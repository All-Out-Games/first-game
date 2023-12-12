using AO;

public class Boss : Component
{
    public const float TICK = 0.5f;

    [Serialized] public int WorldIndex;
    [Serialized] public int BossIndex;
    [Serialized] public string Name;

    public BossDefinition Definition;
    public int AmountToWin => Definition.Difficulty * 20;
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
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 0, RequiredLevel = 1,  Difficulty = 1,  Reward = 5 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 1, RequiredLevel = 3,  Difficulty = 3,  Reward = 10 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 2, RequiredLevel = 7,  Difficulty = 7,  Reward = 30 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 3, RequiredLevel = 12, Difficulty = 12, Reward = 120 },
        new BossDefinition(){ WorldIndex = 0, IndexInWorld = 4, RequiredLevel = 19, Difficulty = 19, Reward = 600 },

        // world 2
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 0, RequiredLevel = 22, Difficulty = 22, Reward = 2700 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 1, RequiredLevel = 26, Difficulty = 26, Reward = 13600 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 2, RequiredLevel = 32, Difficulty = 32, Reward = 68000 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 3, RequiredLevel = 40, Difficulty = 40, Reward = 340000 },
        new BossDefinition(){ WorldIndex = 1, IndexInWorld = 4, RequiredLevel = 49, Difficulty = 49, Reward = 1700000 },

        // world 3
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 0, RequiredLevel = 55, Difficulty = 55, Reward = 7800000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 1, RequiredLevel = 63, Difficulty = 63, Reward = 39000000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 2, RequiredLevel = 72, Difficulty = 72, Reward = 195000000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 3, RequiredLevel = 84, Difficulty = 84, Reward = 975000000 },
        new BossDefinition(){ WorldIndex = 2, IndexInWorld = 4, RequiredLevel = 99, Difficulty = 99, Reward = 4800000000 },
    };
}

public class BossDefinition
{
    public int    WorldIndex;
    public int    IndexInWorld;
    public int    RequiredLevel;
    public double Reward;
    public int    Difficulty;
}

