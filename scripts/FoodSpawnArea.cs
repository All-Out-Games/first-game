using AO;

public class FoodAreaDefinition
{
    public string Id;
    public string Name;
    public string[] FoodsToSpawn;
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

    public int[] FoodCounts;

    public float TotalArea;
    public int CurrentSpawned;

    public override void Start()
    {
        AreaDefinition = AreaDefinitions.FirstOrDefault(a => a.Id == ZoneId);
        if (AreaDefinition == null)
        {
            Log.Error($"Failed to find area definition for zone ID '{ZoneId}'");
        }

        FoodCounts = new int[AreaDefinition.FoodsToSpawn.Length];

        foreach (var child in Entity.Children)
        {
            Zone zone = child.GetComponent<Zone>();
            if (zone == null) continue;
            zone.ZoneId = Entity.Name;
            TotalArea += zone.Entity.Scale.X * zone.Entity.Scale.Y;
        }
    }

    public override void Update()
    {
        if (Network.IsClient) return;
        if (AreaDefinition == null) return;

        var currentDensity = CurrentSpawned / TotalArea;
        if (currentDensity < Density)
        {
            // spawn the thing with the lowest current count
            int lowestFoodValue = int.MaxValue;
            int lowestFoodIndex = -1;
            for (int i = 0; i < FoodCounts.Length; i++)
            {
                if (FoodCounts[i] < lowestFoodValue)
                {
                    lowestFoodValue = FoodCounts[i];
                    lowestFoodIndex = i;
                }
            }

            var newFoodEntity = Entity.Instantiate(FoodPrefabs);
            var food = newFoodEntity.GetComponent<Food>();
            food.FoodId = AreaDefinition.FoodsToSpawn[lowestFoodIndex];
            food.FoodIndexInAreaDefinition = lowestFoodIndex;
            newFoodEntity.Position = Zone.GetRandomPointInZones(Entity.Name);
            Network.Spawn(newFoodEntity);

            CurrentSpawned++;
            FoodCounts[lowestFoodIndex] += 1;

            food.OnEat += (Food food) =>
            {
                CurrentSpawned--;
                FoodCounts[food.FoodIndexInAreaDefinition] -= 1;
            };
        }
    }
}