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
            Zone z = child.GetComponent<Zone>();
            if (z == null) continue;
            z.ZoneId = Entity.Name;
            TotalArea += z.Entity.Scale.X * z.Entity.Scale.Y;
        }
    }

    public override void Update()
    {
        if (Network.IsClient) return;

        var currentDensity = CurrentSpawned / TotalArea;
        if (currentDensity < Density)
        {
            var newFood = Entity.Instantiate(FoodPrefabs);
            newFood.Position = Zone.GetRandomPointInZones(Entity.Name);
            Network.Spawn(newFood);

            CurrentSpawned++;

            newFood.GetComponent<Food>().OnEat += () =>
            {
                CurrentSpawned--;
            };
        }
    }
}