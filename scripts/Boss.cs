using AO;

public class Boss : Component
{
    [Serialized] public int WorldIndex;
    [Serialized] public int BossIndex;
    [Serialized] public string Name;

    public FatPlayer CurrentlyBattling;
    public Spine_Animator SpineAnimator;
    public BossDefinition Definition;
    public int AmountToWin => Definition.AmountPerClick * 30;
    public double Reward => Definition.Reward;

    public void NotifyRequirementNotMetLocal(FatPlayer player, string statName, int level, int required)
    {
        if (!player.IsLocal)
        {
            return;
        }
        Notifications.Show($"Your {statName} level ({level}) must be at least {required}.");
    }

    public override void Start()
    {
        Definition = BossDefinitions.FirstOrDefault(d => d.WorldIndex == WorldIndex && d.IndexInWorld == BossIndex);
        if (Definition == null)
        {
            Log.Error($"Failed to find boss index {BossIndex} for world index {WorldIndex}");
            return;
        }

        SpineAnimator = Entity.GetComponent<Spine_Animator>();
        if (SpineAnimator != null)
        {
            Util.Assert(SpineAnimator.SpineInstance != null);
            SpineAnimator.SpineInstance.SetAnimation("Idle", true);
            SpineAnimator.SpineInstance.SetSkin(Definition.SpineSkin);
            SpineAnimator.SpineInstance.RefreshSkins();
            SpineAnimator.SetCrewchsia(BossIndex);
        }

        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract += (Player p) =>
        {
            var player = (FatPlayer) p;

            var meetsReq = true;
            if ((Definition.RequiredStatFlags & BossRequiredStat.MouthSize) != 0 && player.MouthSizeLevel < Definition.RequiredLevel)
            {
                meetsReq = false;
                NotifyRequirementNotMetLocal(player, "Mouth Size", player.MouthSizeLevel, Definition.RequiredLevel);
            }
            if ((Definition.RequiredStatFlags & BossRequiredStat.ClickPower) != 0 && player.ClickPowerLevel < Definition.RequiredLevel)
            {
                meetsReq = false;
                NotifyRequirementNotMetLocal(player, "Click Power", player.ClickPowerLevel, Definition.RequiredLevel);
            }
            if ((Definition.RequiredStatFlags & BossRequiredStat.StomachSize) != 0 && player.StomachSizeLevel < Definition.RequiredLevel)
            {
                meetsReq = false;
                NotifyRequirementNotMetLocal(player, "Stomach Size", player.StomachSizeLevel, Definition.RequiredLevel);
            }

            if (player.BossInteractCooldownTimer > 0)
            {
                meetsReq = false;
            }

            if (!meetsReq)
            {
                return;
            }

            if (Network.IsServer) {
                Log.Info("Starting boss fight with " + player.Name);
                var pos = Entity.Position;
                if (player.Entity.Position.X < Entity.Position.X) {
                    pos.X -= 2;
                }
                else {
                    pos.X += 2;
                }
                player.CallClient_StartBossFight(Entity.NetworkId, pos);
            }
        };

        interactable.CanUseCallback = (Player p) =>
        {
            var player = (FatPlayer) p;
            if (CurrentlyBattling.Alive())
            {
                return false;
            }
            if (player.IsBusy)
            {
                return false;
            }
            return true;
        };
    }

    public static BossDefinition[] BossDefinitions = new BossDefinition[]
    {
        // world 1
        new BossDefinition(){ Id = "Food Eater",              WorldIndex = 0, IndexInWorld = 0, Reward = 5,          AmountPerClick = 2,  TimeBetweenClicks = 0.1, RequiredLevel = 3,   RequiredStatFlags = BossRequiredStat.MouthSize,                                                              SpineSkin = "001_FAT/boss_Skins/world 1/food_eater" },
        new BossDefinition(){ Id = "Chef",                    WorldIndex = 0, IndexInWorld = 1, Reward = 10,         AmountPerClick = 3,  TimeBetweenClicks = 0.1, RequiredLevel = 5,   RequiredStatFlags = BossRequiredStat.ClickPower,                                                             SpineSkin = "001_FAT/boss_Skins/world 1/chef" },
        new BossDefinition(){ Id = "Food Critic",             WorldIndex = 0, IndexInWorld = 2, Reward = 30,         AmountPerClick = 7,  TimeBetweenClicks = 0.1, RequiredLevel = 8,   RequiredStatFlags = BossRequiredStat.StomachSize,                                                            SpineSkin = "001_FAT/boss_Skins/world 1/food_critic" },
        new BossDefinition(){ Id = "Fast Food Worker",        WorldIndex = 0, IndexInWorld = 3, Reward = 120,        AmountPerClick = 10, TimeBetweenClicks = 0.1, RequiredLevel = 12,  RequiredStatFlags = BossRequiredStat.MouthSize | BossRequiredStat.ClickPower,                                SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Guy Fieri (Look-a-like)", WorldIndex = 0, IndexInWorld = 4, Reward = 600,        AmountPerClick = 17, TimeBetweenClicks = 0.1, RequiredLevel = 19,  RequiredStatFlags = BossRequiredStat.MouthSize | BossRequiredStat.ClickPower | BossRequiredStat.StomachSize, SpineSkin = "001_FAT/boss_Skins/world 1/guy_fiery" },

        // world 2
        new BossDefinition(){ Id = "Fries/Burger Guy",        WorldIndex = 1, IndexInWorld = 0, Reward = 2700,       AmountPerClick = 19, TimeBetweenClicks = 0.1, RequiredLevel = 21,  RequiredStatFlags = BossRequiredStat.StomachSize,                                                            SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Cake Guy",                WorldIndex = 1, IndexInWorld = 1, Reward = 13600,      AmountPerClick = 23, TimeBetweenClicks = 0.1, RequiredLevel = 25,  RequiredStatFlags = BossRequiredStat.ClickPower,                                                             SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Hot Dog Guy",             WorldIndex = 1, IndexInWorld = 2, Reward = 68000,      AmountPerClick = 30, TimeBetweenClicks = 0.1, RequiredLevel = 32,  RequiredStatFlags = BossRequiredStat.MouthSize,                                                              SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Pizza Guy",               WorldIndex = 1, IndexInWorld = 3, Reward = 340000,     AmountPerClick = 42, TimeBetweenClicks = 0.1, RequiredLevel = 44,  RequiredStatFlags = BossRequiredStat.StomachSize | BossRequiredStat.ClickPower,                              SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Banana Guy",              WorldIndex = 1, IndexInWorld = 4, Reward = 1700000,    AmountPerClick = 56, TimeBetweenClicks = 0.1, RequiredLevel = 58,  RequiredStatFlags = BossRequiredStat.MouthSize | BossRequiredStat.ClickPower | BossRequiredStat.StomachSize, SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },

        // world 3
        new BossDefinition(){ Id = "Soda Lover",              WorldIndex = 2, IndexInWorld = 0, Reward = 7800000,    AmountPerClick = 55, TimeBetweenClicks = 0.1, RequiredLevel = 165, RequiredStatFlags = BossRequiredStat.ClickPower,                                                             SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Sushi Chef",              WorldIndex = 2, IndexInWorld = 1, Reward = 39000000,   AmountPerClick = 63, TimeBetweenClicks = 0.1, RequiredLevel = 189, RequiredStatFlags = BossRequiredStat.MouthSize,                                                              SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Candy Crime Boss",        WorldIndex = 2, IndexInWorld = 2, Reward = 195000000,  AmountPerClick = 72, TimeBetweenClicks = 0.1, RequiredLevel = 216, RequiredStatFlags = BossRequiredStat.StomachSize,                                                            SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Pizza Prince",            WorldIndex = 2, IndexInWorld = 3, Reward = 975000000,  AmountPerClick = 84, TimeBetweenClicks = 0.1, RequiredLevel = 252, RequiredStatFlags = BossRequiredStat.MouthSize | BossRequiredStat.StomachSize,                               SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
        new BossDefinition(){ Id = "Food Wizard",             WorldIndex = 2, IndexInWorld = 4, Reward = 4800000000, AmountPerClick = 99, TimeBetweenClicks = 0.1, RequiredLevel = 297, RequiredStatFlags = BossRequiredStat.MouthSize | BossRequiredStat.ClickPower | BossRequiredStat.StomachSize, SpineSkin = "001_FAT/boss_Skins/world 1/fastfood_worker" },
    };
}

public enum BossRequiredStat
{
    MouthSize   = 1 << 0,
    ClickPower  = 1 << 1,
    StomachSize = 1 << 2,
}

public class BossDefinition
{
    public string Id;

    public int    WorldIndex;
    public int    IndexInWorld;
    public BossRequiredStat RequiredStatFlags;
    public int    RequiredLevel;
    public double Reward;
    public double TimeBetweenClicks;
    public int    AmountPerClick;
    public string SpineSkin;
}

