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
            "Apple",
            "broccoli",
            "HotDog",
            "Underwear",
            "Burger",
        }},
        new FoodAreaDefinition(){Id = "Zone2", Name = "Zone2", FoodsToSpawn = new string[]{
            "Pizza",
            "Popcorn",
            "Watermelon",
            "fire_hydrant_normal",
            "Potted_Tree",
        }},
        new FoodAreaDefinition(){Id = "Zone3", Name = "Zone3", FoodsToSpawn = new string[]{
            "Picnick_Table_Empty",
            "Garbage_Bins_Unclean",
            "Car",
            "tesla",
            "monster",
            "plane_clean",
        }},
    };

    [Serialized] public string ZoneId;
    [Serialized] public Prefab FoodPrefabs;
    [Serialized] public float Density;

    public FoodAreaDefinition AreaDefinition;

    public float TotalArea;
    public int CurrentSpawned;

    public override void Start()
    {
        AreaDefinition = AreaDefinitions.FirstOrDefault(a => a.Id == ZoneId);
        if (AreaDefinition == null)
        {
            Log.Error($"Failed to find area definition for zone ID '{ZoneId}'");
        }

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
            var newFoodEntity = Entity.Instantiate(FoodPrefabs);
            var food = newFoodEntity.GetComponent<Food>();
            var rng = new Random();
            var foodIndex = rng.Next(AreaDefinition.FoodsToSpawn.Length);
            food.FoodId = AreaDefinition.FoodsToSpawn[foodIndex];
            Log.Info($"Zone {ZoneId} spawning {food.FoodId}");
            newFoodEntity.Position = Zone.GetRandomPointInZones(Entity.Name);
            Network.Spawn(newFoodEntity);

            CurrentSpawned++;

            food.OnEat += () =>
            {
                CurrentSpawned--;
            };
        }
    }
}