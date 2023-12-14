using AO;
using TinyJson;

public partial class FatPlayer : Player 
{
    public const int MinStomachSize = 10;

    private double _trophies;
    private double _coins;
    private double _food;

    // these are levels, not the modified values, so we just use an int
    public int _maxFoodLevel;
    public int _mouthSizeLevel;
    public int _chewSpeedLevel;
    public int _rebirth;

    public int _maxEquippedPets = 3;
    public int MaxEquippedPets
    {
        get => _maxEquippedPets;
        set
        {
            _maxEquippedPets = value;
            if (Network.IsServer) {
                Save.SetInt(this, "MaxEquippedPets", value);
                CallClient_NotifyMaxEquippedPetsUpdate(value);
            }
        }
    }

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

    public int PlayerLevel => 1 + _maxFoodLevel + _mouthSizeLevel + _chewSpeedLevel;

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
            if (BossAccumulator >= Boss.TICK)
            {
                BossProgress += CurrentBoss.Definition.Difficulty;
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
            Trophies = Save.GetDouble(this, "Trophies", 0);
            Coins = Save.GetDouble(this, "Coins", 0);
            Food = Save.GetDouble(this, "Food", 0);
            MaxFoodLevel = Save.GetInt(this, "MaxFood", 0);
            MouthSizeLevel = Save.GetInt(this, "MouthSize", 0);
            ChewSpeedLevel = Save.GetInt(this, "ChewSpeed", 0);
            Rebirth = Save.GetInt(this, "Rebirth", 0);

            var zones = Save.GetString(this, "UnlockedZones", "[]");
            CallClient_LoadZoneData(zones);

            var pets = Save.GetString(this, "AllPets", "[]");
            CallClient_LoadPetData(pets);

            MaxEquippedPets = Save.GetInt(this, "MaxEquippedPets", 3);
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
                CallClient_NotifyRebirthUpdate(value);
            }
        }
    }

    public double RebirthCashMultiplier => global::Rebirth.Instance.GetRebirthData(Rebirth).CashMultiplier;

    public double ModifiedChewSpeed   => CalculateModifiedStat(PetData.StatModifierKind.ChewSpeed,   ChewSpeedByLevel  [Math.Clamp(_chewSpeedLevel, 0, ChewSpeedByLevel.Length-1)].Value);
    public double ModifiedMouthSize   => CalculateModifiedStat(PetData.StatModifierKind.MouthSize,   MouthSizeByLevel  [Math.Clamp(_mouthSizeLevel, 0, MouthSizeByLevel.Length-1)].Value);
    public double ModifiedStomachSize => CalculateModifiedStat(PetData.StatModifierKind.StomachSize, StomachSizeByLevel[Math.Clamp(_maxFoodLevel,   0, StomachSizeByLevel.Length-1)].Value);

    public double CalculateModifiedStat(PetData.StatModifierKind kind, double baseValue)
    {
        double summedMultipliers = 0.0;
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
                if (modifier.Kind == kind)
                {
                    summedMultipliers += modifier.MultiplyValue - 1.0;
                }
            }
        }
        return baseValue + baseValue * summedMultipliers;
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

    [ClientRpc]
    public void DeletePet(string id)
    {
        PetManager.DeletePet(id);
    }

    [ServerRpc]
    public void RequestEquipPet(string id)
    {
        var equippedPetsCount = PetManager.OwnedPets.Count(p => p.Equipped);
        if (equippedPetsCount >= MaxEquippedPets) {
            return;
        }

        CallClient_EquipPet(id);
    }

    [ServerRpc]
    public void RequestUnequipPet(string id)
    {
        CallClient_UnequipPet(id);
    }

    [ServerRpc]
    public void RequestDeletePet(string id)
    {
        CallClient_DeletePet(id);
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

    [ClientRpc] public void NotifyTrophiesUpdate(double val)  { if (Network.IsClient) Trophies       = val; }
    [ClientRpc] public void NotifyCoinsUpdate(double val)     { if (Network.IsClient) Coins          = val; }
    [ClientRpc] public void NotifyFoodUpdate(double val)      { if (Network.IsClient) Food           = val; }
    [ClientRpc] public void NotifyMaxFoodUpdate(int val)      { if (Network.IsClient) MaxFoodLevel   = val; }
    [ClientRpc] public void NotifyMouthSizeUpdate(int val)    { if (Network.IsClient) MouthSizeLevel = val; }
    [ClientRpc] public void NotifyChewSpeedUpdate(int val)    { if (Network.IsClient) ChewSpeedLevel = val; }
    [ClientRpc] public void NotifyRebirthUpdate(int val)      { if (Network.IsClient) Rebirth        = val; }
    [ClientRpc] public void NotifyMaxEquippedPetsUpdate(int val) { if (Network.IsClient) _maxEquippedPets = val; }

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
        new UpgradeStatAndCost(){ Value = 1,  Cost = 5 },
        new UpgradeStatAndCost(){ Value = 6,  Cost = 58 },
        new UpgradeStatAndCost(){ Value = 11, Cost = 66 },
        new UpgradeStatAndCost(){ Value = 16, Cost = 76 },
        new UpgradeStatAndCost(){ Value = 21, Cost = 87 },
        new UpgradeStatAndCost(){ Value = 26, Cost = 101 },
        new UpgradeStatAndCost(){ Value = 31, Cost = 116 },
        new UpgradeStatAndCost(){ Value = 36, Cost = 133 },
        new UpgradeStatAndCost(){ Value = 41, Cost = 153 },
        new UpgradeStatAndCost(){ Value = 46, Cost = 176 },
        new UpgradeStatAndCost(){ Value = 51, Cost = 202 },
        new UpgradeStatAndCost(){ Value = 56, Cost = 233 },
        new UpgradeStatAndCost(){ Value = 61, Cost = 268 },
        new UpgradeStatAndCost(){ Value = 66, Cost = 308 },
        new UpgradeStatAndCost(){ Value = 71, Cost = 354 },
        new UpgradeStatAndCost(){ Value = 76, Cost = 407 },
        new UpgradeStatAndCost(){ Value = 81, Cost = 468 },
        new UpgradeStatAndCost(){ Value = 86, Cost = 538 },
        new UpgradeStatAndCost(){ Value = 91, Cost = 619 },
        new UpgradeStatAndCost(){ Value = 96, Cost = 712 },

        new UpgradeStatAndCost(){ Value = 101, Cost = 2300 },
        new UpgradeStatAndCost(){ Value = 106, Cost = 2760 },
        new UpgradeStatAndCost(){ Value = 111, Cost = 3312 },
        new UpgradeStatAndCost(){ Value = 116, Cost = 3975 },
        new UpgradeStatAndCost(){ Value = 121, Cost = 4770 },
        new UpgradeStatAndCost(){ Value = 126, Cost = 5724 },
        new UpgradeStatAndCost(){ Value = 131, Cost = 6869 },
        new UpgradeStatAndCost(){ Value = 136, Cost = 8242 },
        new UpgradeStatAndCost(){ Value = 141, Cost = 9891 },
        new UpgradeStatAndCost(){ Value = 146, Cost = 11869 },
        new UpgradeStatAndCost(){ Value = 151, Cost = 14243 },
        new UpgradeStatAndCost(){ Value = 156, Cost = 17091 },
        new UpgradeStatAndCost(){ Value = 161, Cost = 20509 },
        new UpgradeStatAndCost(){ Value = 166, Cost = 24611 },
        new UpgradeStatAndCost(){ Value = 171, Cost = 29533 },
        new UpgradeStatAndCost(){ Value = 176, Cost = 35440 },
        new UpgradeStatAndCost(){ Value = 181, Cost = 42528 },
        new UpgradeStatAndCost(){ Value = 186, Cost = 51034 },
        new UpgradeStatAndCost(){ Value = 191, Cost = 61240 },
        new UpgradeStatAndCost(){ Value = 196, Cost = 73489 },
        new UpgradeStatAndCost(){ Value = 201, Cost = 88186 },
        new UpgradeStatAndCost(){ Value = 206, Cost = 105824 },
        new UpgradeStatAndCost(){ Value = 211, Cost = 126988 },
        new UpgradeStatAndCost(){ Value = 216, Cost = 152386 },
        new UpgradeStatAndCost(){ Value = 221, Cost = 182863 },
        new UpgradeStatAndCost(){ Value = 226, Cost = 219436 },
        new UpgradeStatAndCost(){ Value = 231, Cost = 263323 },
        new UpgradeStatAndCost(){ Value = 236, Cost = 315987 },
        new UpgradeStatAndCost(){ Value = 241, Cost = 379185 },
        new UpgradeStatAndCost(){ Value = 246, Cost = 455022 },
    };

    public static UpgradeStatAndCost[] ChewSpeedByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 1,    Cost = 5 },
        new UpgradeStatAndCost(){ Value = 1.05, Cost = 46 },
        new UpgradeStatAndCost(){ Value = 1.1,  Cost = 53 },
        new UpgradeStatAndCost(){ Value = 1.15, Cost = 61 },
        new UpgradeStatAndCost(){ Value = 1.2,  Cost = 70 },
        new UpgradeStatAndCost(){ Value = 1.25, Cost = 80 },
        new UpgradeStatAndCost(){ Value = 1.3,  Cost = 93 },
        new UpgradeStatAndCost(){ Value = 1.35, Cost = 106 },
        new UpgradeStatAndCost(){ Value = 1.4,  Cost = 122 },
        new UpgradeStatAndCost(){ Value = 1.45, Cost = 141 },
        new UpgradeStatAndCost(){ Value = 1.5,  Cost = 162 },
        new UpgradeStatAndCost(){ Value = 1.55, Cost = 186 },
        new UpgradeStatAndCost(){ Value = 1.6,  Cost = 214 },
        new UpgradeStatAndCost(){ Value = 1.65, Cost = 246 },
        new UpgradeStatAndCost(){ Value = 1.7,  Cost = 283 },
        new UpgradeStatAndCost(){ Value = 1.75, Cost = 325 },
        new UpgradeStatAndCost(){ Value = 1.8,  Cost = 374 },
        new UpgradeStatAndCost(){ Value = 1.85, Cost = 430 },
        new UpgradeStatAndCost(){ Value = 1.9,  Cost = 495 },
        new UpgradeStatAndCost(){ Value = 1.95, Cost = 569 },

        new UpgradeStatAndCost(){ Value = 2,    Cost = 1917 },
        new UpgradeStatAndCost(){ Value = 2.05, Cost = 2300 },
        new UpgradeStatAndCost(){ Value = 2.1,  Cost = 2760 },
        new UpgradeStatAndCost(){ Value = 2.15, Cost = 3312 },
        new UpgradeStatAndCost(){ Value = 2.2,  Cost = 3975 },
        new UpgradeStatAndCost(){ Value = 2.25, Cost = 4770 },
        new UpgradeStatAndCost(){ Value = 2.3,  Cost = 5724 },
        new UpgradeStatAndCost(){ Value = 2.35, Cost = 6869 },
        new UpgradeStatAndCost(){ Value = 2.4,  Cost = 8242 },
        new UpgradeStatAndCost(){ Value = 2.45, Cost = 9891 },
        new UpgradeStatAndCost(){ Value = 2.5,  Cost = 11869 },
        new UpgradeStatAndCost(){ Value = 2.55, Cost = 14243 },
        new UpgradeStatAndCost(){ Value = 2.6,  Cost = 17091 },
        new UpgradeStatAndCost(){ Value = 2.65, Cost = 20509 },
        new UpgradeStatAndCost(){ Value = 2.7,  Cost = 24611 },
        new UpgradeStatAndCost(){ Value = 2.75, Cost = 29533 },
        new UpgradeStatAndCost(){ Value = 2.8,  Cost = 35440 },
        new UpgradeStatAndCost(){ Value = 2.85, Cost = 42528 },
        new UpgradeStatAndCost(){ Value = 2.9,  Cost = 51034 },
        new UpgradeStatAndCost(){ Value = 2.95, Cost = 61240 },
        new UpgradeStatAndCost(){ Value = 3,    Cost = 73489 },
        new UpgradeStatAndCost(){ Value = 3.05, Cost = 88186 },
        new UpgradeStatAndCost(){ Value = 3.1,  Cost = 105824 },
        new UpgradeStatAndCost(){ Value = 3.15, Cost = 126988 },
        new UpgradeStatAndCost(){ Value = 3.2,  Cost = 152386 },
        new UpgradeStatAndCost(){ Value = 3.25, Cost = 182863 },
        new UpgradeStatAndCost(){ Value = 3.3,  Cost = 219436 },
        new UpgradeStatAndCost(){ Value = 3.35, Cost = 263323 },
        new UpgradeStatAndCost(){ Value = 3.4,  Cost = 315987 },
        new UpgradeStatAndCost(){ Value = 3.45, Cost = 379185 },
    };

    public static UpgradeStatAndCost[] StomachSizeByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 10,  Cost = 5 },
        new UpgradeStatAndCost(){ Value = 15,  Cost = 35 },
        new UpgradeStatAndCost(){ Value = 20,  Cost = 40 },
        new UpgradeStatAndCost(){ Value = 25,  Cost = 46 },
        new UpgradeStatAndCost(){ Value = 30,  Cost = 52 },
        new UpgradeStatAndCost(){ Value = 35,  Cost = 60 },
        new UpgradeStatAndCost(){ Value = 40,  Cost = 69 },
        new UpgradeStatAndCost(){ Value = 45,  Cost = 80 },
        new UpgradeStatAndCost(){ Value = 50,  Cost = 92 },
        new UpgradeStatAndCost(){ Value = 55,  Cost = 106 },
        new UpgradeStatAndCost(){ Value = 60,  Cost = 121 },
        new UpgradeStatAndCost(){ Value = 65,  Cost = 140 },
        new UpgradeStatAndCost(){ Value = 70,  Cost = 161 },
        new UpgradeStatAndCost(){ Value = 75,  Cost = 185 },
        new UpgradeStatAndCost(){ Value = 80,  Cost = 212 },
        new UpgradeStatAndCost(){ Value = 85,  Cost = 244 },
        new UpgradeStatAndCost(){ Value = 90,  Cost = 281 },
        new UpgradeStatAndCost(){ Value = 95,  Cost = 323 },
        new UpgradeStatAndCost(){ Value = 100, Cost = 371 },
        new UpgradeStatAndCost(){ Value = 105, Cost = 427 },

        new UpgradeStatAndCost(){ Value = 110, Cost = 1534 },
        new UpgradeStatAndCost(){ Value = 115, Cost = 1840 },
        new UpgradeStatAndCost(){ Value = 120, Cost = 2208 },
        new UpgradeStatAndCost(){ Value = 125, Cost = 2650 },
        new UpgradeStatAndCost(){ Value = 130, Cost = 3180 },
        new UpgradeStatAndCost(){ Value = 135, Cost = 3816 },
        new UpgradeStatAndCost(){ Value = 140, Cost = 4579 },
        new UpgradeStatAndCost(){ Value = 145, Cost = 5495 },
        new UpgradeStatAndCost(){ Value = 150, Cost = 6594 },
        new UpgradeStatAndCost(){ Value = 155, Cost = 7913 },
        new UpgradeStatAndCost(){ Value = 160, Cost = 9495 },
        new UpgradeStatAndCost(){ Value = 165, Cost = 11394 },
        new UpgradeStatAndCost(){ Value = 170, Cost = 13673 },
        new UpgradeStatAndCost(){ Value = 175, Cost = 16407 },
        new UpgradeStatAndCost(){ Value = 180, Cost = 19689 },
        new UpgradeStatAndCost(){ Value = 185, Cost = 23627 },
        new UpgradeStatAndCost(){ Value = 190, Cost = 28352 },
        new UpgradeStatAndCost(){ Value = 195, Cost = 34022 },
        new UpgradeStatAndCost(){ Value = 200, Cost = 40827 },
        new UpgradeStatAndCost(){ Value = 205, Cost = 48992 },
        new UpgradeStatAndCost(){ Value = 210, Cost = 58791 },
        new UpgradeStatAndCost(){ Value = 215, Cost = 70549 },
        new UpgradeStatAndCost(){ Value = 220, Cost = 84659 },
        new UpgradeStatAndCost(){ Value = 225, Cost = 101591 },
        new UpgradeStatAndCost(){ Value = 230, Cost = 121909 },
        new UpgradeStatAndCost(){ Value = 235, Cost = 146290 },
        new UpgradeStatAndCost(){ Value = 240, Cost = 175549 },
        new UpgradeStatAndCost(){ Value = 245, Cost = 210658 },
        new UpgradeStatAndCost(){ Value = 250, Cost = 252790 },
        new UpgradeStatAndCost(){ Value = 255, Cost = 303348 },
    };
}
