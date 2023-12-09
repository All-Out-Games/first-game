using AO;
using TinyJson;

public partial class FatPlayer : Player 
{
    public const int MinStomachSize = 10;

    private double _trophies;
    private double _coins;
    private double _food;

    // these are levels, not the modified values, so we just use an int
    private int    _maxFoodLevel;
    private int    _mouthSizeLevel;
    private int    _chewSpeedLevel;
    private int    _rebirth;

    public Food FoodBeingEaten;
    
    private PetManager _petManager;
    public PetManager PetManager 
    {
        get 
        {
            if (_petManager == null) {
                _petManager = Entity.AddComponent<PetManager>();
                _petManager.Player = this;
            }
            return _petManager;
        }
    }

    public Boss CurrentBoss;
    public double BossProgress;
    public double MyProgress;
    public double BossAccumulator;

    [AOIgnore] public List<string> UnlockedZones = new List<string>();

    public override void Start()
    {
    }

    [ClientRpc]
    public void BossFightOver(bool won)
    {
        if (CurrentBoss == null) {
            Log.Info("Boss fight over but no boss?");
            return;
        }

        if (won) 
        {
            Trophies += CurrentBoss.Reward;
            if (IsLocal)
            {
                Notifications.Show("You won the boss fight!");
            }
        }
        else 
        {
            if (IsLocal)
            {
                Notifications.Show("You lost the boss fight!");
            }
        }


        CurrentBoss = null;
        this.RemoveFreezeReason("BossFight");
    }

    public override void Update()
    {
        foreach (var pet in Pet.AllPets)
        {
            if (pet.OwnerId != Entity.NetworkId)
            {
                continue;
            }
            pet.Arrived = false;
        }

        if (CurrentBoss != null)
        {
            if (this.IsMouseUpLeft()) 
            {
                MyProgress += Math.Max(1, (MouthSizeLevel + ChewSpeedLevel) / 2);
            }

            BossAccumulator += Time.DeltaTime;
            if (BossAccumulator >= CurrentBoss.Tick) 
            {
                BossProgress += CurrentBoss.Increment;
                BossAccumulator = 0;
            }

            if (Network.IsServer)
            {
                if (MyProgress >= CurrentBoss.AmountToWin || BossProgress >= CurrentBoss.AmountToWin) 
                {
                    CallClient_BossFightOver(MyProgress > BossProgress);
                }
            }
        }

        if (FoodBeingEaten != null)
        {
            var chewRect = UI.GetPlayerRect(this);
            chewRect = chewRect.Grow(50, 50, 0, 50).Offset(0, 10);

            UI.Image(chewRect, null, Vector4.White, new UI.NineSlice());

            var chewProgress = Math.Min(1.0, FoodBeingEaten.EatingTime / FoodBeingEaten.ConsumptionTime);
            var chewProgressRect = chewRect.SubRect(0, 0, (float)chewProgress, 1, 0, 0, 0, 0);
            UI.Image(chewProgressRect, null, Vector4.HSVLerp(Vector4.Red, Vector4.Green, (float)chewProgress), new UI.NineSlice());
        }

        if (this.IsLocal) 
        {
            if (FoodBeingEaten != null && Input.GetKeyDown(Input.Keycode.KEYCODE_ESCAPE, true))
            {
                CallServer_GiveUpEating();
            }

            if (CurrentBoss != null && Input.GetKeyDown(Input.Keycode.KEYCODE_ESCAPE, true))
            {
                CallServer_GiveUpBossFight();
            }
        }
    }

    public override void OnDestroy()
    {
        if (FoodBeingEaten != null && Network.IsServer) {
            FoodBeingEaten.CallClient_FinishEating(false);
        }

        Pet.AllPets.RemoveAll(p => p.OwnerId == this.Entity.NetworkId);
    }

    public void OnLoad()
    {
        if (Network.IsServer)
        {
            Trophies = Save.GetInt(this, "Trophies", 0);
            Coins = Save.GetInt(this, "Coins", 0);
            Food = Save.GetInt(this, "Food", 0);
            MaxFoodLevel = Save.GetInt(this, "MaxFood", 0);
            MouthSizeLevel = Save.GetInt(this, "MouthSize", 0);
            ChewSpeedLevel = Save.GetInt(this, "ChewSpeed", 0);
            Rebirth = Save.GetInt(this, "Rebirth", 0);

            var zones = Save.GetString(this, "UnlockedZones", "[]");
            CallClient_LoadZoneData(zones);

            var pets = Save.GetString(this, "AllPets", "[]");
            CallClient_LoadPetData(pets);
        }
    }

    [ServerRpc]
    public void GiveUpEating()
    {
        if (FoodBeingEaten != null)
        {
            FoodBeingEaten.CallClient_FinishEating(false);
        }
    }

    [ServerRpc]
    public void GiveUpBossFight()
    {
        if (CurrentBoss != null)
        {
            CallClient_BossFightOver(false);
        }
    }

    [ClientRpc]
    public void StartBossFight(ulong bossNetworkId)
    {
        Log.Info("Really starting boss fight");
        var entity = Entity.FindByNetworkId(bossNetworkId);
        if (entity == null) return;

        Log.Info("Found boss entity and actually starting the fight this time for real");
        CurrentBoss = entity.GetComponent<Boss>();
        this.AddFreezeReason("BossFight");
        
        MyProgress = 0;
        BossProgress = 0;
        BossAccumulator = 0;
    }

    public double Trophies
    { 
        get => _trophies; 
        set 
        { 
            _trophies = value; 
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "Trophies", value);
                CallClient_NotifyTrophiesUpdate(value);
            }
        } 
    }

    public double Coins
    { 
        get => _coins; 
        set 
        { 
            _coins = value; 
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "Coins", value);
                CallClient_NotifyCoinsUpdate(value);
            }
        } 
    }

    public double Food
    { 
        get => _food; 
        set 
        { 
            _food = value; 
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "Food", value);
                CallClient_NotifyFoodUpdate(value);
            }
        } 
    }

    public int MaxFoodLevel
    { 
        get => _maxFoodLevel;
        set 
        { 
            _maxFoodLevel = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "MaxFood", value);
                CallClient_NotifyMaxFoodUpdate(value);
            }
        } 
    }

    public int MouthSizeLevel
    { 
        get => _mouthSizeLevel;
        set 
        { 
            _mouthSizeLevel = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "MouthSize", value);
                CallClient_NotifyMouthSizeUpdate(value);
            }
        } 
    }

    public int ChewSpeedLevel
    { 
        get => _chewSpeedLevel;
        set 
        { 
            _chewSpeedLevel = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "ChewSpeed", value);
                CallClient_NotifyChewSpeedUpdate(value);
            }
        } 
    }

    public int Rebirth
    {
        get => _rebirth;
        set
        {
            _rebirth = value;
            if (Network.IsServer)
            {
                Save.SetInt(this, "Rebirth", value);
            }
        }
    }

    public double ModifiedChewSpeed => CalculateModifiedStat(PetData.StatModifierKind.ChewSpeedMultiply, PetData.StatModifierKind.ChewSpeedAdd, ChewSpeedByLevel  [Math.Clamp(_chewSpeedLevel, 0, ChewSpeedByLevel.Length-1)].Value);
    public double ModifiedMouthSize => CalculateModifiedStat(PetData.StatModifierKind.MouthSizeMultiply, PetData.StatModifierKind.MouthSizeAdd, MouthSizeByLevel  [Math.Clamp(_mouthSizeLevel, 0, MouthSizeByLevel.Length-1)].Value);
    public double ModifiedMaxFood   => CalculateModifiedStat(PetData.StatModifierKind.MaxFoodMultiply,   PetData.StatModifierKind.MaxFoodAdd,   StomachSizeByLevel[Math.Clamp(_maxFoodLevel,   0, StomachSizeByLevel.Length-1)].Value);

    public double CalculateModifiedStat(PetData.StatModifierKind multiplyKind, PetData.StatModifierKind addKind, double baseValue)
    {
        foreach (var pet in PetManager.OwnedPets)
        {
            if (!pet.Equipped)
            {
                continue;
            }
            var defn = pet.GetDefinition();
            if (defn == null)
            {
                continue;
            }
            foreach (var modifier in defn.StatModifiers)
            {
                if (modifier.Kind == addKind)
                {
                    baseValue += modifier.AddValue;
                }
            }
        }

        double modified = baseValue;
        foreach (var pet in PetManager.OwnedPets)
        {
            if (!pet.Equipped)
            {
                continue;
            }
            var defn = pet.GetDefinition();
            if (defn == null)
            {
                continue;
            }
            foreach (var modifier in defn.StatModifiers)
            {
                if (modifier.Kind == multiplyKind)
                {
                    modified *= modifier.MultiplyValue;
                }
            }
        }
        return modified;
    }

    [AOIgnore] public Dictionary<string, int> ZoneCosts = new Dictionary<string, int>() 
    {
        { "zone0", 0 },
        { "zone1", 10 },
        { "zone2", 20 },
        { "zone3", 30 },
        { "zone4", 40 },
        { "zone5", 50 },
        { "zone6", 60 },
        { "zone7", 70 },
        { "zone8", 80 },
        { "zone9", 90 },
    };

    public void PurchaseZone(string id)
    {
        if (!Network.IsServer) return;

        if (!ZoneCosts.TryGetValue(id, out var cost)) 
        {
            Log.Error($"Could not find cost for zone {id}");
            return;
        }

        if (Trophies < cost) 
        {
            Log.Error($"Not enough trophies to purchase zone {id}");
            return;
        }

        Trophies -= cost;
        CallClient_NotifyZoneUnlocked(id);
        Save.SetString(this, "UnlockedZones", JSONWriter.ToJson(UnlockedZones));
    }

    [ClientRpc]
    public void LoadZoneData(string data)
    {
        UnlockedZones = JSONParser.FromJson<List<string>>(data);
    }

    [ClientRpc]
    public void NotifyZoneUnlocked(string id)
    {
        UnlockedZones.Add(id);
    }

    [ClientRpc]
    public void LoadPetData(string data)
    {
        PetManager.LoadPetData(data);
    }

    [ClientRpc]
    public void AddPet(string petId, string petDefinitionId)
    {
        PetManager.AddPet(petId, petDefinitionId);
    }

    [ClientRpc]
    public void EquipPet(string petId)
    {
        PetManager.EquipPet(petId);
    }

    [ClientRpc]
    public void UnequipPet(string id)
    {
        PetManager.UnequipPet(id);
    }

    [ServerRpc]
    public void RequestEquipPet(string id)
    {
        CallClient_EquipPet(id);
    }

    [ServerRpc]
    public void RequestUnequipPet(string id)
    {
        CallClient_UnequipPet(id);
    }

    [ClientRpc]
    public void DoRebirth(int rebirth)
    {
        var rbd = global::Rebirth.Instance.GetRebirthData(rebirth);
        
        this.Rebirth = rebirth;
        Notifications.Show($"You have been reborn! You are now a {rbd.RankName}!");
        // TODO show an anim or something
    }

    [ServerRpc]
    public void RequestRebirth()
    {
        var rbd = global::Rebirth.Instance.GetRebirthData(this.Rebirth + 1);
        if (this.Trophies < rbd.TrophiesCost) {
            // TODO alert user
            return;
        }

        this.Trophies -= rbd.TrophiesCost;
        this.Rebirth += 1;
        this.Coins = 0;
        this.ChewSpeedLevel = 0;
        this.MouthSizeLevel = 0;
        this.MaxFoodLevel = 0;
        
        CallClient_DoRebirth(this.Rebirth);
    }

    [ClientRpc] public void NotifyTrophiesUpdate(double val)  { if (Network.IsClient) Trophies = val;  }
    [ClientRpc] public void NotifyCoinsUpdate(double val)     { if (Network.IsClient) Coins = val;     }
    [ClientRpc] public void NotifyFoodUpdate(double val)      { if (Network.IsClient) { Food = val; Log.Info("Updating food via RPC"); } }
    [ClientRpc] public void NotifyMaxFoodUpdate(int val)   { if (Network.IsClient) MaxFoodLevel = val;   }
    [ClientRpc] public void NotifyMouthSizeUpdate(int val) { if (Network.IsClient) MouthSizeLevel = val; }
    [ClientRpc] public void NotifyChewSpeedUpdate(int val) { if (Network.IsClient) ChewSpeedLevel = val; }

    [ServerRpc]
    public void RequestPurchaseStomachSize()
    {
        if (Network.IsClient) return;
        if (MaxFoodLevel >= StomachSizeByLevel.Length) return;
        double cost = StomachSizeByLevel[MaxFoodLevel].Cost;
        if (Coins < cost) return;

        Coins -= cost;
        MaxFoodLevel += 1;
    }

    [ServerRpc]
    public void RequestPurchaseMouthSize()
    {
        if (Network.IsClient) return;
        if (MouthSizeLevel >= MouthSizeByLevel.Length) return;
        double cost = MouthSizeByLevel[MouthSizeLevel].Cost;
        if (Coins < cost) return;

        Coins -= cost;
        MouthSizeLevel += 1;
    }

    [ServerRpc]
    public void RequestPurchaseChewSpeed()
    {
        if (Network.IsClient) return;
        if (ChewSpeedLevel >= ChewSpeedByLevel.Length) return;
        double cost = ChewSpeedByLevel[ChewSpeedLevel].Cost;
        if (Coins < cost) return;

        Coins -= cost;
        ChewSpeedLevel += 1;
    }

    [ServerRpc]
    public void RequestPurchaseItem(string id)
    {
        Shop.Instance.Purchase(this, id);
    }

    public struct UpgradeStatAndCost
    {
        public double Value;
        public double Cost;
    }

    public static UpgradeStatAndCost[] MouthSizeByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 1,  Cost = 125 },
        new UpgradeStatAndCost(){ Value = 6,  Cost = 169 },
        new UpgradeStatAndCost(){ Value = 11, Cost = 228 },
        new UpgradeStatAndCost(){ Value = 16, Cost = 308 },
        new UpgradeStatAndCost(){ Value = 21, Cost = 415 },
        new UpgradeStatAndCost(){ Value = 26, Cost = 561 },
        new UpgradeStatAndCost(){ Value = 31, Cost = 757 },
        new UpgradeStatAndCost(){ Value = 36, Cost = 1022 },
        new UpgradeStatAndCost(){ Value = 41, Cost = 1379 },
        new UpgradeStatAndCost(){ Value = 46, Cost = 1862 },
        new UpgradeStatAndCost(){ Value = 51, Cost = 2513 },
        new UpgradeStatAndCost(){ Value = 56, Cost = 3393 },
        new UpgradeStatAndCost(){ Value = 61, Cost = 4581 },
        new UpgradeStatAndCost(){ Value = 66, Cost = 6184 },
        new UpgradeStatAndCost(){ Value = 71, Cost = 8348 },
        new UpgradeStatAndCost(){ Value = 76, Cost = 11270 },
        new UpgradeStatAndCost(){ Value = 81, Cost = 15214 },
        new UpgradeStatAndCost(){ Value = 86, Cost = 20539 },
        new UpgradeStatAndCost(){ Value = 91, Cost = 27728 },
        new UpgradeStatAndCost(){ Value = 96, Cost = 37433 },

        new UpgradeStatAndCost(){ Value = 101, Cost = 50534 },
        new UpgradeStatAndCost(){ Value = 106, Cost = 68221 },
        new UpgradeStatAndCost(){ Value = 111, Cost = 92099 },
        new UpgradeStatAndCost(){ Value = 116, Cost = 124333 },
        new UpgradeStatAndCost(){ Value = 121, Cost = 167850 },
        new UpgradeStatAndCost(){ Value = 126, Cost = 226597 },
        new UpgradeStatAndCost(){ Value = 131, Cost = 305906 },
        new UpgradeStatAndCost(){ Value = 136, Cost = 412973 },
        new UpgradeStatAndCost(){ Value = 141, Cost = 557514 },
        new UpgradeStatAndCost(){ Value = 146, Cost = 752643 },
        new UpgradeStatAndCost(){ Value = 151, Cost = 1016069 },
        new UpgradeStatAndCost(){ Value = 156, Cost = 1371693 },
        new UpgradeStatAndCost(){ Value = 161, Cost = 1851785 },
        new UpgradeStatAndCost(){ Value = 166, Cost = 2499910 },
        new UpgradeStatAndCost(){ Value = 171, Cost = 3374878 },
        new UpgradeStatAndCost(){ Value = 176, Cost = 4556086 },
        new UpgradeStatAndCost(){ Value = 181, Cost = 6150716 },
        new UpgradeStatAndCost(){ Value = 186, Cost = 8303467 },
        new UpgradeStatAndCost(){ Value = 191, Cost = 11209680 },
        new UpgradeStatAndCost(){ Value = 196, Cost = 15133068 },
        new UpgradeStatAndCost(){ Value = 201, Cost = 20429642 },
        new UpgradeStatAndCost(){ Value = 206, Cost = 27580016 },
        new UpgradeStatAndCost(){ Value = 211, Cost = 37233022 },
        new UpgradeStatAndCost(){ Value = 216, Cost = 50264580 },
        new UpgradeStatAndCost(){ Value = 221, Cost = 67857183 },
        new UpgradeStatAndCost(){ Value = 226, Cost = 91607197 },
        new UpgradeStatAndCost(){ Value = 231, Cost = 123669716 },
        new UpgradeStatAndCost(){ Value = 236, Cost = 166954117 },
        new UpgradeStatAndCost(){ Value = 241, Cost = 225388058 },
        new UpgradeStatAndCost(){ Value = 246, Cost = 304273878 },
    };

    public static UpgradeStatAndCost[] ChewSpeedByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 1,    Cost = 100 },
        new UpgradeStatAndCost(){ Value = 1.05, Cost = 130 },
        new UpgradeStatAndCost(){ Value = 1.1,  Cost = 169 },
        new UpgradeStatAndCost(){ Value = 1.15, Cost = 220 },
        new UpgradeStatAndCost(){ Value = 1.2,  Cost = 286 },
        new UpgradeStatAndCost(){ Value = 1.25, Cost = 371 },
        new UpgradeStatAndCost(){ Value = 1.3,  Cost = 483 },
        new UpgradeStatAndCost(){ Value = 1.35, Cost = 627 },
        new UpgradeStatAndCost(){ Value = 1.4,  Cost = 816 },
        new UpgradeStatAndCost(){ Value = 1.45, Cost = 1060 },
        new UpgradeStatAndCost(){ Value = 1.5,  Cost = 1379 },
        new UpgradeStatAndCost(){ Value = 1.55, Cost = 1792 },
        new UpgradeStatAndCost(){ Value = 1.6,  Cost = 2330 },
        new UpgradeStatAndCost(){ Value = 1.65, Cost = 3029 },
        new UpgradeStatAndCost(){ Value = 1.7,  Cost = 3937 },
        new UpgradeStatAndCost(){ Value = 1.75, Cost = 5119 },
        new UpgradeStatAndCost(){ Value = 1.8,  Cost = 6654 },
        new UpgradeStatAndCost(){ Value = 1.85, Cost = 8650 },
        new UpgradeStatAndCost(){ Value = 1.9,  Cost = 11246 },
        new UpgradeStatAndCost(){ Value = 1.95, Cost = 14619 },

        new UpgradeStatAndCost(){ Value = 2,    Cost = 19005 },
        new UpgradeStatAndCost(){ Value = 2.05, Cost = 24706 },
        new UpgradeStatAndCost(){ Value = 2.1,  Cost = 32118 },
        new UpgradeStatAndCost(){ Value = 2.15, Cost = 41754 },
        new UpgradeStatAndCost(){ Value = 2.2,  Cost = 54280 },
        new UpgradeStatAndCost(){ Value = 2.25, Cost = 70564 },
        new UpgradeStatAndCost(){ Value = 2.3,  Cost = 91733 },
        new UpgradeStatAndCost(){ Value = 2.35, Cost = 119253 },
        new UpgradeStatAndCost(){ Value = 2.4,  Cost = 155029 },
        new UpgradeStatAndCost(){ Value = 2.45, Cost = 201538 },
        new UpgradeStatAndCost(){ Value = 2.5,  Cost = 262000 },
        new UpgradeStatAndCost(){ Value = 2.55, Cost = 340599 },
        new UpgradeStatAndCost(){ Value = 2.6,  Cost = 442779 },
        new UpgradeStatAndCost(){ Value = 2.65, Cost = 575613 },
        new UpgradeStatAndCost(){ Value = 2.7,  Cost = 748297 },
        new UpgradeStatAndCost(){ Value = 2.75, Cost = 972786 },
        new UpgradeStatAndCost(){ Value = 2.8,  Cost = 1264622 },
        new UpgradeStatAndCost(){ Value = 2.85, Cost = 1644008 },
        new UpgradeStatAndCost(){ Value = 2.9,  Cost = 2137211 },
        new UpgradeStatAndCost(){ Value = 2.95, Cost = 2778374 },
        new UpgradeStatAndCost(){ Value = 3,    Cost = 3611886 },
        new UpgradeStatAndCost(){ Value = 3.05, Cost = 4695452 },
        new UpgradeStatAndCost(){ Value = 3.1,  Cost = 6104088 },
        new UpgradeStatAndCost(){ Value = 3.15, Cost = 7935315 },
        new UpgradeStatAndCost(){ Value = 3.2,  Cost = 10315909 },
        new UpgradeStatAndCost(){ Value = 3.25, Cost = 13410682 },
        new UpgradeStatAndCost(){ Value = 3.3,  Cost = 17433886 },
        new UpgradeStatAndCost(){ Value = 3.35, Cost = 22664052 },
        new UpgradeStatAndCost(){ Value = 3.4,  Cost = 29463268 },
        new UpgradeStatAndCost(){ Value = 3.45, Cost = 38302248 },
    };

    public static UpgradeStatAndCost[] StomachSizeByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 10,  Cost = 75 },
        new UpgradeStatAndCost(){ Value = 15,  Cost = 94 },
        new UpgradeStatAndCost(){ Value = 20,  Cost = 117 },
        new UpgradeStatAndCost(){ Value = 25,  Cost = 146 },
        new UpgradeStatAndCost(){ Value = 30,  Cost = 183 },
        new UpgradeStatAndCost(){ Value = 35,  Cost = 229 },
        new UpgradeStatAndCost(){ Value = 40,  Cost = 286 },
        new UpgradeStatAndCost(){ Value = 45,  Cost = 358 },
        new UpgradeStatAndCost(){ Value = 50,  Cost = 447 },
        new UpgradeStatAndCost(){ Value = 55,  Cost = 559 },
        new UpgradeStatAndCost(){ Value = 60,  Cost = 698 },
        new UpgradeStatAndCost(){ Value = 65,  Cost = 873 },
        new UpgradeStatAndCost(){ Value = 70,  Cost = 1091 },
        new UpgradeStatAndCost(){ Value = 75,  Cost = 1364 },
        new UpgradeStatAndCost(){ Value = 80,  Cost = 1705 },
        new UpgradeStatAndCost(){ Value = 85,  Cost = 2132 },
        new UpgradeStatAndCost(){ Value = 90,  Cost = 2665 },
        new UpgradeStatAndCost(){ Value = 95,  Cost = 3331 },
        new UpgradeStatAndCost(){ Value = 100, Cost = 4163 },
        new UpgradeStatAndCost(){ Value = 105, Cost = 5204 },

        new UpgradeStatAndCost(){ Value = 110, Cost = 6505 },
        new UpgradeStatAndCost(){ Value = 115, Cost = 8132 },
        new UpgradeStatAndCost(){ Value = 120, Cost = 10164 },
        new UpgradeStatAndCost(){ Value = 125, Cost = 12705 },
        new UpgradeStatAndCost(){ Value = 130, Cost = 15882 },
        new UpgradeStatAndCost(){ Value = 135, Cost = 19852 },
        new UpgradeStatAndCost(){ Value = 140, Cost = 24815 },
        new UpgradeStatAndCost(){ Value = 145, Cost = 31019 },
        new UpgradeStatAndCost(){ Value = 150, Cost = 38774 },
        new UpgradeStatAndCost(){ Value = 155, Cost = 48468 },
        new UpgradeStatAndCost(){ Value = 160, Cost = 60585 },
        new UpgradeStatAndCost(){ Value = 165, Cost = 75731 },
        new UpgradeStatAndCost(){ Value = 170, Cost = 94663 },
        new UpgradeStatAndCost(){ Value = 175, Cost = 118329 },
        new UpgradeStatAndCost(){ Value = 180, Cost = 147911 },
        new UpgradeStatAndCost(){ Value = 185, Cost = 184889 },
        new UpgradeStatAndCost(){ Value = 190, Cost = 231112 },
        new UpgradeStatAndCost(){ Value = 195, Cost = 288889 },
        new UpgradeStatAndCost(){ Value = 200, Cost = 361112 },
        new UpgradeStatAndCost(){ Value = 205, Cost = 451390 },
        new UpgradeStatAndCost(){ Value = 210, Cost = 564237 },
        new UpgradeStatAndCost(){ Value = 215, Cost = 705297 },
        new UpgradeStatAndCost(){ Value = 220, Cost = 881621 },
        new UpgradeStatAndCost(){ Value = 225, Cost = 1102026 },
        new UpgradeStatAndCost(){ Value = 230, Cost = 1377532 },
        new UpgradeStatAndCost(){ Value = 235, Cost = 1721916 },
        new UpgradeStatAndCost(){ Value = 240, Cost = 2152394 },
        new UpgradeStatAndCost(){ Value = 245, Cost = 2690493 },
        new UpgradeStatAndCost(){ Value = 250, Cost = 3363116 },
        new UpgradeStatAndCost(){ Value = 255, Cost = 4203895 },
    };
}
