using AO;

public class FoodAreaDefinition
{
    public string Id;
    public string Name;
    public string[] FoodsToSpawn;
}

public class FoodSpawnSlot
{
    public Vector2 Position;
    public bool HasFood;
    public float TimeWithoutFood;
}

public class FoodSpawnArea : Component
{
    public static List<FoodAreaDefinition> AreaDefinitions = new()
    {
        new FoodAreaDefinition(){Id = "Zone1", Name = "Zone1", FoodsToSpawn = new string[]{
            "Apple1",
            "broccoli1",
            "HotDog1",
            "Underwear1",
            "Burger1",
            "Pizza1",
            "Popcorn1",
            "Watermelon1",
            "fire_hydrant_normal1",
        }},
        new FoodAreaDefinition(){Id = "Zone2", Name = "Zone2", FoodsToSpawn = new string[]{
            "Underwear2",
            "Burger2",
            "Pizza2",
            "Popcorn2",
            "Watermelon2",
            "fire_hydrant_normal2",
            "Potted_Tree2",
            "Picnick_Table_Empty2",
            "Garbage_Bins_Unclean2",
        }},
        new FoodAreaDefinition(){Id = "Zone3", Name = "Zone3", FoodsToSpawn = new string[]{
            "Watermelon3",
            "fire_hydrant_normal3",
            "Potted_Tree3",
            "Picnick_Table_Empty3",
            "Garbage_Bins_Unclean3",
            "Car3",
            "tesla3",
            "monster3",
            "plane_clean3",
        }},
    };

    [Serialized] public string ZoneId;
    [Serialized] public Prefab FoodPrefabs;
    [Serialized] public float Density;

    public FoodAreaDefinition AreaDefinition;

    public List<FoodSpawnSlot> SpawnSlots = new();

    public int[] FoodCounts;
    public int ActiveFoodCount;

    public Vector2 HalfCellSize;

    public override void Start()
    {
        AreaDefinition = AreaDefinitions.FirstOrDefault(a => a.Id == ZoneId);
        if (AreaDefinition == null)
        {
            Log.Error($"Failed to find area definition for zone ID '{ZoneId}'");
        }

        FoodCounts = new int[AreaDefinition.FoodsToSpawn.Length];

        if (Density == 0)
        {
            Log.Error("Density was 0 which is invalid. Setting to 0.5.");
            Density = 0.5f;
        }
        var step = new Vector2(1.0f, 1.0f);
        HalfCellSize = step * 0.5f;
        foreach (var child in Entity.Children)
        {
            Zone zone = child.GetComponent<Zone>();
            if (zone == null) continue;
            zone.ZoneId = Entity.Name;

            var halfSize = zone.Entity.Scale * 0.5f;
            var min = zone.Entity.Position - halfSize;
            var max = zone.Entity.Position + halfSize;
            var cursor = min;
            while (cursor.Y <= max.Y)
            {
                while (cursor.X <= max.X)
                {
                    var slot = new FoodSpawnSlot();
                    slot.Position = cursor;
                    slot.TimeWithoutFood = 999999;
                    SpawnSlots.Add(slot);
                    cursor.X += step.X;
                }
                cursor.X = min.X;
                cursor.Y += step.Y;
            }
        }
    }

    public override void Update()
    {
        if (Network.IsClient) return;
        if (AreaDefinition == null) return;

        float currentDensity = (float)ActiveFoodCount / (float)SpawnSlots.Count;

        if (currentDensity < Density)
        {
            var rng = new Random();

            int startIndex = rng.Next(SpawnSlots.Count);
            for (int i = startIndex; i < (startIndex + SpawnSlots.Count); i++)
            {
                int index = i % SpawnSlots.Count;
                var slot = SpawnSlots[index];
                if (slot.HasFood)
                {
                    continue;
                }

                // spawn the thing with the lowest current count
                int lowestFoodValue = int.MaxValue;
                int lowestFoodIndex = -1;
                for (int j = 0; j < FoodCounts.Length; j++)
                {
                    if (FoodCounts[j] < lowestFoodValue)
                    {
                        lowestFoodValue = FoodCounts[j];
                        lowestFoodIndex = j;
                    }
                }

                var newFoodEntity = Entity.Instantiate(FoodPrefabs);
                var food = newFoodEntity.GetComponent<Food>();
                food.FoodId = AreaDefinition.FoodsToSpawn[lowestFoodIndex];
                food.FoodIndexInAreaDefinition = lowestFoodIndex;
                newFoodEntity.Position = slot.Position;
                newFoodEntity.X += ((float)rng.NextDouble() * 2 - 1) * HalfCellSize.X;
                newFoodEntity.Y += ((float)rng.NextDouble() * 2 - 1) * HalfCellSize.Y;
                Network.Spawn(newFoodEntity);

                FoodCounts[lowestFoodIndex] += 1;
                slot.HasFood = true;
                ActiveFoodCount += 1;

                var thisSlot = slot;
                food.OnEat += (Food food) =>
                {
                    ActiveFoodCount -= 1;
                    thisSlot.HasFood = false;
                    FoodCounts[food.FoodIndexInAreaDefinition] -= 1;
                };

                break;
            }
        }
    }
}