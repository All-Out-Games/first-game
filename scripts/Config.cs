using System.Net;
using AO;

public partial class Config : System<Config>
{
    public override void Start()
    {
        if (Network.IsServer)
            FetchAllConfigs();
    }

    public float Accum;

    public override void Update()
    {
        Accum += Time.DeltaTime;

        if (Accum >= 10f && Network.IsServer)
        {
            Accum = 0f;
            FetchAllConfigs();
        }
    }

    public void FetchAllConfigs()
    {
        HTTP.Get("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Food", (body) => {
            CallClient_LoadFood(body);
        });

        HTTP.Get("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Upgrades", (body) => {
            CallClient_LoadUpgrades(body);
        });

        HTTP.Get("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Rebirth", (body) => {
            CallClient_LoadRebirth(body);
        });

        HTTP.Get("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Bosses", (body) => {
            CallClient_LoadBosses(body);
        });

        // var eggsConfig = client.DownloadString("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Eggs");
        // var petsConfig = client.DownloadString("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Pets");
        // var questsConfig = client.DownloadString("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Quests");
        // var milestonesConfig = client.DownloadString("https://docs.google.com/spreadsheets/d/1fARsHvr1b1tB-CbabXmhcazUqBi48lnCSV4293v3Tss/gviz/tq?tqx=out:csv&sheet=Milestones");
    }

    [ClientRpc]
    public static void LoadFood(string csv)
    {
        FoodConfig.Load(csv);
        foreach (var w in FoodConfig.Instance.Worlds)
        {
            foreach (var f in w.FoodItems)
            {
                var foodDef = Food.FoodDefinitions.FirstOrDefault(d => d.Id == f.Name);
                if (foodDef == null)
                {
                    continue;
                }

                foodDef.ConsumptionTime = f.ConsumptionTime;
                foodDef.RequiredMouthSize = f.RequiredMouthSize;
                foodDef.StomachSpace = f.RequiredStomachSapce;
                foodDef.SellValue = f.FoodValue;
            }
        }
    }

    [ClientRpc]
    public static void LoadUpgrades(string csv)
    {
        UpgradeConfig.Load(csv);
        FatPlayer.MouthSizeByLevel = UpgradeConfig.Instance.MouthSizeLevels.ToArray();
        FatPlayer.ClickPowerByLevel = UpgradeConfig.Instance.ClickPowerLevels.ToArray();
        FatPlayer.StomachSizeByLevel = UpgradeConfig.Instance.StomachSizeLevels.ToArray();
    }

    [ClientRpc]
    public static void LoadRebirth(string csv)
    {
        RebirthConfig.Load(csv);
        Rebirth.RebirthData = RebirthConfig.Instance.Ranks;
    }

    [ClientRpc]
    public static void LoadBosses(string csv)
    {
        BossesConfig.Load(csv);

        foreach (var b in BossesConfig.Instance.Bosses)
        {
            var bossDef = Boss.BossDefinitions.FirstOrDefault(d => d.Id == b.Name);
            if (bossDef == null)
            {
                continue;
            }

            bossDef.RequiredLevel = b.RequiredLevel;
            bossDef.TimeBetweenClicks = b.ReciprocalCPS;
            bossDef.AmountPerClick = b.BossClickPower;
            bossDef.Reward = b.RewardTrophies;

            if (b.RequiredSkills.Contains("MOUTH"))
            {
                bossDef.RequiredStatFlags |= BossRequiredStat.MouthSize;
            }
            if (b.RequiredSkills.Contains("CHEW"))
            {
                bossDef.RequiredStatFlags |= BossRequiredStat.ClickPower;
            }
            if (b.RequiredSkills.Contains("STOMACH"))
            {
                bossDef.RequiredStatFlags |= BossRequiredStat.StomachSize;
            }
        }
    }

}

public struct BossesConfig
{
    public static BossesConfig Instance;
    public static void Load(string csv)
    {
        Instance = new BossesConfig() { Bosses = new List<Boss>() };

        var lines = csv.Split('\n');
        foreach (var l in lines)
        {
            var columns = l.Split(',');
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = columns[i].Trim('"');
            }

            if (columns[1] == "Index") continue; // Header
            if (columns[1] == "Boss") continue; // Header
            if (columns[1].StartsWith("World")) continue; // Header

            Instance.Bosses.Add(new Boss()
            {
                Name = columns[1],
                RequiredSkills = columns[3].Split('+').ToList(),
                RequiredLevel = int.Parse(columns[4]),
                RewardTrophies = double.Parse(columns[5]),
                BossClickPower = int.Parse(columns[6]),
                ReciprocalCPS = float.Parse(columns[7]),
            });
        }
    }

    public List<Boss> Bosses;
    public struct Boss
    {
        public string Name;
        public List<string> RequiredSkills;
        public int RequiredLevel;
        public double RewardTrophies;
        public int BossClickPower;
        public float ReciprocalCPS;
    }
}

public struct RebirthConfig
{
    public static RebirthConfig Instance;

    public static void Load(string csv)
    {
        Instance = new RebirthConfig() { Ranks = new List<RebirthData>() };

        var lines = csv.Split('\n');
        foreach (var l in lines)
        {
            var columns = l.Split(',');
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = columns[i].Trim('"');
            }

            if (columns[1] == "Index") continue; // Header

            Instance.Ranks.Add(new RebirthData()
            {
                RankName = columns[2],
                TrophiesCost = double.Parse(columns[3]),
                CashMultiplier = float.Parse(columns[4]),
            });
        }
    }

    public List<RebirthData> Ranks;
}

public struct UpgradeConfig
{
    public static UpgradeConfig Instance;

    public static void Load(string csv)
    {
        var lines = csv.Split('\n');

        Instance = new UpgradeConfig() { MouthSizeLevels = new List<FatPlayer.UpgradeStatAndCost>(), ClickPowerLevels = new List<FatPlayer.UpgradeStatAndCost>(), StomachSizeLevels = new List<FatPlayer.UpgradeStatAndCost>() };
        for (int i = 0; i < lines.Length; i++)
        {
            if (i < 5) continue; // Some identifiers are missing from the csv when downloaded

            string l = lines[i];
            var columns = l.Split(',');
            for (int j = 0; j < columns.Length; j++)
            {
                columns[j] = columns[j].Trim('"');
            }

            Instance.MouthSizeLevels.Add(new FatPlayer.UpgradeStatAndCost()
            {
                Value = double.Parse(columns[2]),
                Cost = double.Parse(columns[3]),
            });

            Instance.ClickPowerLevels.Add(new FatPlayer.UpgradeStatAndCost()
            {
                Value = double.Parse(columns[7]),
                Cost = double.Parse(columns[8]),
            });

            Instance.StomachSizeLevels.Add(new FatPlayer.UpgradeStatAndCost()
            {
                Value = double.Parse(columns[12]),
                Cost = double.Parse(columns[13]),
            });
        }
    }

    public List<FatPlayer.UpgradeStatAndCost> MouthSizeLevels;
    public List<FatPlayer.UpgradeStatAndCost> ClickPowerLevels;
    public List<FatPlayer.UpgradeStatAndCost> StomachSizeLevels;
}

public struct FoodConfig
{
    public static FoodConfig Instance;

    public static void Load(string csv)
    {
        var lines = csv.Split('\n');

        Instance = new FoodConfig() { Worlds = new List<World>() };
        World? currentWorld = null;
        foreach (var l in lines)
        {
            var columns = l.Split(',');
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = columns[i].Trim('"');
            }

            if (columns[1].StartsWith("World"))
            {
                if (currentWorld != null)
                {
                    Instance.Worlds.Add(currentWorld.Value);
                }

                currentWorld = new World() { Name = columns[1], FoodItems = new List<World.FoodItem>() };
                continue;
            }

            if (currentWorld == null) continue; // No world yet (header
            if (columns[1] == "Item Name") continue; // Header

            currentWorld.Value.FoodItems.Add(new World.FoodItem()
            {
                Name = columns[1],
                ConsumptionTime = int.Parse(columns[3]),
                RequiredMouthSize = int.Parse(columns[4]),
                RequiredStomachSapce = int.Parse(columns[5]),
                FoodValue = int.Parse(columns[6]),
            });
        }

        if (currentWorld != null)
        {
            Instance.Worlds.Add(currentWorld.Value);
        }
    }

    public List<World> Worlds;
    
    public struct World
    {
        public string Name;
        public List<FoodItem> FoodItems;

        public struct FoodItem
        {
            public string Name;
            public int ConsumptionTime;
            public int RequiredMouthSize;
            public int RequiredStomachSapce;
            public int FoodValue;
        }
    }
}