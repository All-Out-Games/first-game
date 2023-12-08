using AO;

public class FoodSpawnArea : Component
{
    [Serialized] public Prefab FoodPrefabs;
    [Serialized] public float Density;

    public float TotalArea;
    public int CurrentSpawned;

    public override void Start()
    {
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

        var currentDensity = CurrentSpawned / TotalArea;
        if (currentDensity < Density)
        {
            var newFoodEntity = Entity.Instantiate(FoodPrefabs);
            var food = newFoodEntity.GetComponent<Food>();
            var rng = new Random();
            food.FoodDefinitionIndex = rng.Next(Food.FoodDefinitions.Count);
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