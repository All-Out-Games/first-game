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
            "apple",
            "feastable_bar",
            "PB&J_Sandwich",
            "popcorn",
            "grimace_shake_small",
            "milk_jug",
            "doggy_poop_bin",
            "fire_hydrant",
            "trash_bag",
            "infinity_gauntlet",
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

    public const double TimeBetweenCarePackages = 10; // todo(josh): @Incomplete: figure out what we want for this
    public double CarePackageSpawnTimeAcc;
    public Interactable SpawnedCarePackage;

    public List<Zone> ChildZones = new();
    public List<float> ChildZoneWeights = new();

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
        float totalWeight = 0;
        foreach (var child in Entity.Children)
        {
            Zone zone = child.GetComponent<Zone>();
            if (zone == null) continue;
            zone.ZoneId = Entity.Name;

            ChildZones.Add(zone);
            ChildZoneWeights.Add(totalWeight);
            totalWeight += zone.Entity.Scale.X * zone.Entity.Scale.Y;

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

        if (totalWeight > 0)
        {
            for (int i = 0; i < ChildZoneWeights.Count; i++)
            {
                ChildZoneWeights[i] /= totalWeight;
            }
        }
    }

    public override void Update()
    {
        if (Network.IsClient) return;
        if (AreaDefinition == null) return;

        // spawn care packages
        if (ChildZoneWeights.Count > 0)
        {
            Util.Assert(Network.IsServer);

            if (SpawnedCarePackage.Alive())
            {
                CarePackageSpawnTimeAcc = 0;
            }
            else
            {
                CarePackageSpawnTimeAcc += Time.DeltaTime;
                if (CarePackageSpawnTimeAcc >= TimeBetweenCarePackages)
                {
                    CarePackageSpawnTimeAcc = 0;
                    var rng = new Random();
                    float rnd = (float)rng.NextDouble();
                    int zoneToUse = -1;
                    for (int i = 0; i < ChildZoneWeights.Count; i++)
                    {
                        if (rnd >= ChildZoneWeights[i])
                        {
                            zoneToUse = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (zoneToUse >= 0)
                    {
                        Zone zone = ChildZones[zoneToUse];
                        var halfSize = zone.Entity.Scale * 0.5f;
                        var min = zone.Entity.Position - halfSize;
                        var max = zone.Entity.Position + halfSize;
                        var pos = Util.RandomPositionInBox(min, max, rng);
                        var carePackageEntity = Entity.Instantiate(References.Instance.CarePackagePrefab);
                        carePackageEntity.Position = pos;
                        SpawnedCarePackage = carePackageEntity.GetComponent<Interactable>();
                        Util.Assert(SpawnedCarePackage != null);
                        Network.Spawn(carePackageEntity);
                    }
                }
            }
        }

        // spawn food
        {
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
                    food.WasDynamicallySpawned = true;
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
}