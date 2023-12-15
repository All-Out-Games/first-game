using AO;

public class Boss : Component
{
    [Serialized] public int WorldIndex;
    [Serialized] public int BossIndex;
    [Serialized] public string Name;

    public BossDefinition Definition;
    public int AmountToWin => Definition.AmountPerClick * 30;
    public double Reward => Definition.Reward;

    public override void Start()
    {
        Definition = BossDefinitions.FirstOrDefault(d => d.WorldIndex == WorldIndex && d.IndexInWorld == BossIndex);
        if (Definition == null)
        {
            Log.Error($"Failed to find boss index {BossIndex} for world index {WorldIndex}");
            return;
        }

        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract += (Player p) =>
        {
            var player = (FatPlayer) p;
            if (Network.IsServer) {
                Log.Info("Starting boss fight with " + player.Name);
                player.CallClient_StartBossFight(Entity.NetworkId);
            }
        };

        interactable.CanUseCallback = (Player p) =>
        {
            var player = (FatPlayer) p;
            if (player.FoodBeingEaten != null)
            {
                return false;
            }
            if (player.CurrentBoss != null)
            {
                return false;
            }
            return true;
        };
    }

    public static BossDefinition[] BossDefinitions = new BossDefinition[]
    {
        // world 1
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 0, RequiredLevel = 3,   TimeBetweenClicks = 0.25, AmountPerClick = 1,  Reward = 5 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 1, RequiredLevel = 9,   TimeBetweenClicks = 0.25, AmountPerClick = 3,  Reward = 10 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 2, RequiredLevel = 21,  TimeBetweenClicks = 0.25, AmountPerClick = 7,  Reward = 30 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 3, RequiredLevel = 36,  TimeBetweenClicks = 0.25, AmountPerClick = 12, Reward = 120 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 4, RequiredLevel = 57,  TimeBetweenClicks = 0.25, AmountPerClick = 19, Reward = 600 },

        // world 2
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 0, RequiredLevel = 66,  TimeBetweenClicks = 0.25, AmountPerClick = 22, Reward = 2700 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 1, RequiredLevel = 78,  TimeBetweenClicks = 0.25, AmountPerClick = 26, Reward = 13600 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 2, RequiredLevel = 96,  TimeBetweenClicks = 0.25, AmountPerClick = 32, Reward = 68000 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 3, RequiredLevel = 120, TimeBetweenClicks = 0.25, AmountPerClick = 40, Reward = 340000 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 4, RequiredLevel = 147, TimeBetweenClicks = 0.25, AmountPerClick = 49, Reward = 1700000 },

        // world 3
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 0, RequiredLevel = 165, TimeBetweenClicks = 0.25, AmountPerClick = 55, Reward = 7800000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 1, RequiredLevel = 189, TimeBetweenClicks = 0.25, AmountPerClick = 63, Reward = 39000000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 2, RequiredLevel = 216, TimeBetweenClicks = 0.25, AmountPerClick = 72, Reward = 195000000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 3, RequiredLevel = 252, TimeBetweenClicks = 0.25, AmountPerClick = 84, Reward = 975000000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 4, RequiredLevel = 297, TimeBetweenClicks = 0.25, AmountPerClick = 99, Reward = 4800000000 },
    };
}

public class BossDefinition
{
    public int    WorldIndex;
    public int    IndexInWorld;
    public int    RequiredLevel;
    public double Reward;
    public double TimeBetweenClicks;
    public int    AmountPerClick;
}

