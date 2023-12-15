using AO;
using TinyJson;

public partial class FatPlayer : Player 
{
    public const int MinStomachSize = 10;

    private double _trophies;
    private double _coins;
    private double _amountOfFoodInStomach;
    private double _valueOfFoodInStomach;

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

    public Quest CurrentQuest;

    public Boss CurrentBoss;
    public double BossProgress;
    public double MyProgress;
    public double BossAccumulator;

    public int PlayerLevel => 1 + _maxFoodLevel + _mouthSizeLevel + _chewSpeedLevel;

    public List<string> UnlockedZones = new List<string>();

    public List<StatModifier> TemporaryBuffs = new();

    public override void Start()
    {
    }

    [ClientRpc]
    public void BossFightOver(bool won)
    {
        if (CurrentBoss == null)
        {
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
            if (Network.IsServer)
            {
                if (CurrentQuest != null)
                {
                    CurrentQuest.OnBossBeatenServer(CurrentBoss);
                }
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

    [ServerRpc]
    public void ResetAllProgressCheat()
    {
        Trophies = 0;
        Coins = 0;
        AmountOfFoodInStomach = 0;
        ValueOfFoodInStomach = 0;
        MaxFoodLevel = 0;
        MouthSizeLevel = 0;
        ChewSpeedLevel = 0;
        Rebirth = 0;
        CallClient_DeleteAllPets();
    }

    [ServerRpc]
    public void IncreaseMoneyCheat()
    {
        Coins = (Coins + 5) * 2;
    }

    [ServerRpc]
    public void DecreaseMoneyCheat()
    {
        Coins /= 2;
    }

    [ServerRpc]
    public void IncreaseTrophiesCheat()
    {
        Trophies = (Trophies + 5) * 2;
    }

    [ServerRpc]
    public void DecreaseTrophiesCheat()
    {
        Trophies /= 2;
    }

    [ServerRpc]
    public void ForceCompleteQuestCheat()
    {
        if (CurrentQuest == null)
        {
            return;
        }
        CurrentQuest.ReportProgress(CurrentQuest.ProgressRequired);
    }

    public override void Update()
    {
        if (Input.GetKeyHeld(Input.Keycode.KEYCODE_LEFT_CONTROL)) {
            if (Input.GetKeyHeld(Input.Keycode.KEYCODE_LEFT_SHIFT)) {
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_R)) {
                    CallServer_ResetAllProgressCheat();
                }
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_EQUAL)) {
                    CallServer_IncreaseTrophiesCheat();
                }
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_MINUS)) {
                    CallServer_DecreaseTrophiesCheat();
                }
            }
            else {
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_EQUAL)) {
                    CallServer_IncreaseMoneyCheat();
                }
                if (Input.GetKeyDown(Input.Keycode.KEYCODE_MINUS)) {
                    CallServer_DecreaseMoneyCheat();
                }
            }

            if (Input.GetKeyDown(Input.Keycode.KEYCODE_Q)) {
                CallServer_ForceCompleteQuestCheat();
            }
        }

        if (Network.IsServer)
        {
            if (CurrentQuest != null)
            {
                CurrentQuest.UpdateServer();
            }

            foreach (var buff in TemporaryBuffs)
            {
                buff.TimeLeft -= Time.DeltaTime;
                if (buff.TimeLeft <= 0)
                {
                    CallClient_RemoveTemporaryBuff(buff.Id);
                }
            }
        }

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
            while (Util.Timer(ref BossAccumulator, CurrentBoss.Definition.TimeBetweenClicks))
            {
                BossProgress += CurrentBoss.Definition.AmountPerClick;
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

    public static int LastBuffId;
    public void ServerGiveTemporaryBuff(StatModifierKind kind, float multiplyValue, float duration)
    {
        Util.Assert(Network.IsServer);
        LastBuffId += 1;
        CallClient_GiveTemporaryBuff(LastBuffId, (int)kind, multiplyValue, duration);
    }

    [ClientRpc]
    public void GiveTemporaryBuff(int id, int _kind, float multiplyValue, float duration)
    {
        var kind = (StatModifierKind)_kind;
        var mod = new StatModifier();
        mod.Kind          = kind;
        mod.MultiplyValue = multiplyValue;
        mod.Id            = id;
        mod.TimeLeft      = duration;
        TemporaryBuffs.Add(mod);
    }

    [ClientRpc]
    public void RemoveTemporaryBuff(int id)
    {
        for (int i = 0; i < TemporaryBuffs.Count; i++)
        {
            if (TemporaryBuffs[i].Id == id)
            {
                TemporaryBuffs.RemoveAt(i);
                return;
            }
        }
        Log.Error($"Failed to find buff with id: {id}");
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
            AmountOfFoodInStomach = Save.GetDouble(this, "AmountOfFoodInStomach", 0);
            ValueOfFoodInStomach = Save.GetDouble(this, "ValueOfFoodInStomach", 0);
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

    [ClientRpc]
    public void SyncQuestTimeLeft(float timeLeft)
    {
        if (CurrentQuest == null)
        {
            Log.Error("We didn't have a quest??");
            return;
        }

        CurrentQuest.TimeLeft = timeLeft;
    }

    [ClientRpc]
    public void OnQuestProgressUpdated(int progress)
    {
        if (CurrentQuest == null)
        {
            Log.Error("We didn't have a quest??");
            return;
        }

        CurrentQuest.Progress = progress;
    }

    [ClientRpc]
    public void OnQuestFinished(bool success, string reasonIfFailed)
    {
        if (CurrentQuest == null)
        {
            Log.Error("We didn't have a quest??");
            return;
        }

        if (IsLocal)
        {
            if (success)
            {
                Notifications.Show("Quest completed! :)");
            }
            else
            {
                Notifications.Show($"Quest failed: {reasonIfFailed}");
            }
        }

        CurrentQuest = null;
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

    public double AmountOfFoodInStomach
    { 
        get => _amountOfFoodInStomach;
        set 
        { 
            _amountOfFoodInStomach = value;
            if (Network.IsServer) 
            {
                Save.SetDouble(this, "AmountOfFoodInStomach", value);
                CallClient_NotifyAmountOfFoodInStomachUpdate(value);
            }
        } 
    }

    public double ValueOfFoodInStomach
    {
        get => _valueOfFoodInStomach;
        set
        {
            _valueOfFoodInStomach = value;
            if (Network.IsServer)
            {
                Save.SetDouble(this, "ValueOfFoodInStomach", value);
                CallClient_NotifyValueOfFoodInStomachUpdate(value);
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

    public double BaseChewSpeedValue      => ChewSpeedByLevel  [Math.Clamp(_chewSpeedLevel, 0, ChewSpeedByLevel.Length-1)].Value;
    public double BaseMouthSizeValue      => MouthSizeByLevel  [Math.Clamp(_mouthSizeLevel, 0, MouthSizeByLevel.Length-1)].Value;
    public double BaseStomachSizeValue    => StomachSizeByLevel[Math.Clamp(_maxFoodLevel,   0, StomachSizeByLevel.Length-1)].Value;
    public double BaseCashMultiplierValue => global::Rebirth.Instance.GetRebirthData(Rebirth).CashMultiplier;

    public double ModifiedChewSpeed      => BaseChewSpeedValue      * CalculateTotalMultiplierFromPets(StatModifierKind.ChewSpeed)      * CalculateTotalMultiplierFromBuffs(StatModifierKind.ChewSpeed);
    public double ModifiedMouthSize      => BaseMouthSizeValue      * CalculateTotalMultiplierFromPets(StatModifierKind.MouthSize)      * CalculateTotalMultiplierFromBuffs(StatModifierKind.MouthSize);
    public double ModifiedStomachSize    => BaseStomachSizeValue    * CalculateTotalMultiplierFromPets(StatModifierKind.StomachSize)    * CalculateTotalMultiplierFromBuffs(StatModifierKind.StomachSize);
    public double ModifiedCashMultiplier => BaseCashMultiplierValue * CalculateTotalMultiplierFromPets(StatModifierKind.CashMultiplier) * CalculateTotalMultiplierFromBuffs(StatModifierKind.CashMultiplier);

    public double CalculateTotalMultiplierFromPets(StatModifierKind kind)
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
        return 1 + summedMultipliers;
    }

    public double CalculateTotalMultiplierFromBuffs(StatModifierKind kind)
    {
        double summedMultipliers = 0.0;
        foreach (var buff in TemporaryBuffs)
        {
            if (buff.Kind == kind)
            {
                summedMultipliers += buff.MultiplyValue - 1.0;
            }
        }

        /*
        todo(josh): we could add game passes here like
        foreach (var pass in GamePasses)
        {
            if (pass.Id == "super_chew")
            {
                if (kind == StatModifierKind.ChewSpeed)
                {
                    // 5x chew speed multiplier
                    summedMultipliers += 5.0f - 1.0;
                }
            }
        }
        */

        return 1 + summedMultipliers;
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

    [ClientRpc]
    public void DeleteAllPets()
    {
        PetManager.DeleteAllPets();
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

    [ClientRpc] public void NotifyTrophiesUpdate(double val)                   { if (Network.IsClient) Trophies              = val; }
    [ClientRpc] public void NotifyCoinsUpdate(double val)                      { if (Network.IsClient) Coins                 = val; }
    [ClientRpc] public void NotifyAmountOfFoodInStomachUpdate(double val)      { if (Network.IsClient) AmountOfFoodInStomach = val; }
    [ClientRpc] public void NotifyValueOfFoodInStomachUpdate(double val)       { if (Network.IsClient) ValueOfFoodInStomach  = val; }
    [ClientRpc] public void NotifyMaxFoodUpdate(int val)                       { if (Network.IsClient) MaxFoodLevel          = val; }
    [ClientRpc] public void NotifyMouthSizeUpdate(int val)                     { if (Network.IsClient) MouthSizeLevel        = val; }
    [ClientRpc] public void NotifyChewSpeedUpdate(int val)                     { if (Network.IsClient) ChewSpeedLevel        = val; }
    [ClientRpc] public void NotifyRebirthUpdate(int val)                       { if (Network.IsClient) Rebirth               = val; }
    [ClientRpc] public void NotifyMaxEquippedPetsUpdate(int val)               { if (Network.IsClient) _maxEquippedPets      = val; }

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
        new UpgradeStatAndCost(){ Value = 1,   Cost = 5 },
        new UpgradeStatAndCost(){ Value = 6,   Cost = 7 },
        new UpgradeStatAndCost(){ Value = 11,  Cost = 9 },
        new UpgradeStatAndCost(){ Value = 16,  Cost = 11 },
        new UpgradeStatAndCost(){ Value = 21,  Cost = 15 },
        new UpgradeStatAndCost(){ Value = 26,  Cost = 20 },
        new UpgradeStatAndCost(){ Value = 31,  Cost = 26 },
        new UpgradeStatAndCost(){ Value = 36,  Cost = 35 },
        new UpgradeStatAndCost(){ Value = 41,  Cost = 46 },
        new UpgradeStatAndCost(){ Value = 46,  Cost = 61 },
        new UpgradeStatAndCost(){ Value = 51,  Cost = 80 },
        new UpgradeStatAndCost(){ Value = 56,  Cost = 106 },
        new UpgradeStatAndCost(){ Value = 61,  Cost = 140 },
        new UpgradeStatAndCost(){ Value = 66,  Cost = 185 },
        new UpgradeStatAndCost(){ Value = 71,  Cost = 244 },
        new UpgradeStatAndCost(){ Value = 76,  Cost = 322 },
        new UpgradeStatAndCost(){ Value = 81,  Cost = 425 },
        new UpgradeStatAndCost(){ Value = 86,  Cost = 561 },
        new UpgradeStatAndCost(){ Value = 91,  Cost = 740 },
        new UpgradeStatAndCost(){ Value = 96,  Cost = 977 },
        new UpgradeStatAndCost(){ Value = 101, Cost = 1628 },
        new UpgradeStatAndCost(){ Value = 106, Cost = 2230 },
        new UpgradeStatAndCost(){ Value = 111, Cost = 3055 },
        new UpgradeStatAndCost(){ Value = 116, Cost = 4185 },
        new UpgradeStatAndCost(){ Value = 121, Cost = 5733 },
        new UpgradeStatAndCost(){ Value = 126, Cost = 7855 },
        new UpgradeStatAndCost(){ Value = 131, Cost = 10761 },
        new UpgradeStatAndCost(){ Value = 136, Cost = 14743 },
        new UpgradeStatAndCost(){ Value = 141, Cost = 20197 },
        new UpgradeStatAndCost(){ Value = 146, Cost = 27671 },
        new UpgradeStatAndCost(){ Value = 151, Cost = 37909 },
        new UpgradeStatAndCost(){ Value = 156, Cost = 51935 },
        new UpgradeStatAndCost(){ Value = 161, Cost = 71151 },
        new UpgradeStatAndCost(){ Value = 166, Cost = 97477 },
        new UpgradeStatAndCost(){ Value = 171, Cost = 133543 },
        new UpgradeStatAndCost(){ Value = 176, Cost = 182954 },
        new UpgradeStatAndCost(){ Value = 181, Cost = 250647 },
        new UpgradeStatAndCost(){ Value = 186, Cost = 343386 },
        new UpgradeStatAndCost(){ Value = 191, Cost = 470438 },
        new UpgradeStatAndCost(){ Value = 196, Cost = 644501 },
        new UpgradeStatAndCost(){ Value = 201, Cost = 882966 },
        new UpgradeStatAndCost(){ Value = 206, Cost = 1209663 },
        new UpgradeStatAndCost(){ Value = 211, Cost = 1657239 },
        new UpgradeStatAndCost(){ Value = 216, Cost = 2270417 },
        new UpgradeStatAndCost(){ Value = 221, Cost = 3110471 },
        new UpgradeStatAndCost(){ Value = 226, Cost = 4261346 },
        new UpgradeStatAndCost(){ Value = 231, Cost = 5838044 },
        new UpgradeStatAndCost(){ Value = 236, Cost = 7998120 },
        new UpgradeStatAndCost(){ Value = 241, Cost = 10957424 },
        new UpgradeStatAndCost(){ Value = 246, Cost = 15011671 },
    };

    public static UpgradeStatAndCost[] ChewSpeedByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 1,    Cost = 5 },
        new UpgradeStatAndCost(){ Value = 1.05, Cost = 7 },
        new UpgradeStatAndCost(){ Value = 1.1,  Cost = 8 },
        new UpgradeStatAndCost(){ Value = 1.15, Cost = 11 },
        new UpgradeStatAndCost(){ Value = 1.2,  Cost = 14 },
        new UpgradeStatAndCost(){ Value = 1.25, Cost = 19 },
        new UpgradeStatAndCost(){ Value = 1.3,  Cost = 24 },
        new UpgradeStatAndCost(){ Value = 1.35, Cost = 31 },
        new UpgradeStatAndCost(){ Value = 1.4,  Cost = 41 },
        new UpgradeStatAndCost(){ Value = 1.45, Cost = 53 },
        new UpgradeStatAndCost(){ Value = 1.5,  Cost = 69 },
        new UpgradeStatAndCost(){ Value = 1.55, Cost = 90 },
        new UpgradeStatAndCost(){ Value = 1.6,  Cost = 116 },
        new UpgradeStatAndCost(){ Value = 1.65, Cost = 151 },
        new UpgradeStatAndCost(){ Value = 1.7,  Cost = 197 },
        new UpgradeStatAndCost(){ Value = 1.75, Cost = 256 },
        new UpgradeStatAndCost(){ Value = 1.8,  Cost = 333 },
        new UpgradeStatAndCost(){ Value = 1.85, Cost = 433 },
        new UpgradeStatAndCost(){ Value = 1.9,  Cost = 562 },
        new UpgradeStatAndCost(){ Value = 1.95, Cost = 731 },
        new UpgradeStatAndCost(){ Value = 2,    Cost = 1213 },
        new UpgradeStatAndCost(){ Value = 2.05, Cost = 1637 },
        new UpgradeStatAndCost(){ Value = 2.1,  Cost = 2210 },
        new UpgradeStatAndCost(){ Value = 2.15, Cost = 2984 },
        new UpgradeStatAndCost(){ Value = 2.2,  Cost = 4028 },
        new UpgradeStatAndCost(){ Value = 2.25, Cost = 5438 },
        new UpgradeStatAndCost(){ Value = 2.3,  Cost = 7342 },
        new UpgradeStatAndCost(){ Value = 2.35, Cost = 9911 },
        new UpgradeStatAndCost(){ Value = 2.4,  Cost = 13380 },
        new UpgradeStatAndCost(){ Value = 2.45, Cost = 18063 },
        new UpgradeStatAndCost(){ Value = 2.5,  Cost = 24386 },
        new UpgradeStatAndCost(){ Value = 2.55, Cost = 32921 },
        new UpgradeStatAndCost(){ Value = 2.6,  Cost = 44443 },
        new UpgradeStatAndCost(){ Value = 2.65, Cost = 59998 },
        new UpgradeStatAndCost(){ Value = 2.7,  Cost = 80997 },
        new UpgradeStatAndCost(){ Value = 2.75, Cost = 109346 },
        new UpgradeStatAndCost(){ Value = 2.8,  Cost = 147617 },
        new UpgradeStatAndCost(){ Value = 2.85, Cost = 199283 },
        new UpgradeStatAndCost(){ Value = 2.9,  Cost = 269032 },
        new UpgradeStatAndCost(){ Value = 2.95, Cost = 363194 },
        new UpgradeStatAndCost(){ Value = 3,    Cost = 490311 },
        new UpgradeStatAndCost(){ Value = 3.05, Cost = 661920 },
        new UpgradeStatAndCost(){ Value = 3.1,  Cost = 893593 },
        new UpgradeStatAndCost(){ Value = 3.15, Cost = 1206350 },
        new UpgradeStatAndCost(){ Value = 3.2,  Cost = 1628572 },
        new UpgradeStatAndCost(){ Value = 3.25, Cost = 2198573 },
        new UpgradeStatAndCost(){ Value = 3.3,  Cost = 2968073 },
        new UpgradeStatAndCost(){ Value = 3.35, Cost = 4006899 },
        new UpgradeStatAndCost(){ Value = 3.4,  Cost = 5409313 },
        new UpgradeStatAndCost(){ Value = 3.45, Cost = 7302573 },
    };

    public static UpgradeStatAndCost[] StomachSizeByLevel = new UpgradeStatAndCost[]
    {
        new UpgradeStatAndCost(){ Value = 100, Cost = 5 },
        new UpgradeStatAndCost(){ Value = 110, Cost = 6 },
        new UpgradeStatAndCost(){ Value = 120, Cost = 8 },
        new UpgradeStatAndCost(){ Value = 130, Cost = 10 },
        new UpgradeStatAndCost(){ Value = 140, Cost = 13 },
        new UpgradeStatAndCost(){ Value = 150, Cost = 17 },
        new UpgradeStatAndCost(){ Value = 160, Cost = 22 },
        new UpgradeStatAndCost(){ Value = 170, Cost = 28 },
        new UpgradeStatAndCost(){ Value = 180, Cost = 36 },
        new UpgradeStatAndCost(){ Value = 190, Cost = 46 },
        new UpgradeStatAndCost(){ Value = 200, Cost = 59 },
        new UpgradeStatAndCost(){ Value = 210, Cost = 76 },
        new UpgradeStatAndCost(){ Value = 220, Cost = 97 },
        new UpgradeStatAndCost(){ Value = 230, Cost = 124 },
        new UpgradeStatAndCost(){ Value = 240, Cost = 158 },
        new UpgradeStatAndCost(){ Value = 250, Cost = 203 },
        new UpgradeStatAndCost(){ Value = 260, Cost = 260 },
        new UpgradeStatAndCost(){ Value = 270, Cost = 332 },
        new UpgradeStatAndCost(){ Value = 280, Cost = 425 },
        new UpgradeStatAndCost(){ Value = 290, Cost = 544 },
        new UpgradeStatAndCost(){ Value = 300, Cost = 774 },
        new UpgradeStatAndCost(){ Value = 310, Cost = 1021 },
        new UpgradeStatAndCost(){ Value = 320, Cost = 1348 },
        new UpgradeStatAndCost(){ Value = 330, Cost = 1780 },
        new UpgradeStatAndCost(){ Value = 340, Cost = 2349 },
        new UpgradeStatAndCost(){ Value = 350, Cost = 3101 },
        new UpgradeStatAndCost(){ Value = 360, Cost = 4093 },
        new UpgradeStatAndCost(){ Value = 370, Cost = 5403 },
        new UpgradeStatAndCost(){ Value = 380, Cost = 7132 },
        new UpgradeStatAndCost(){ Value = 390, Cost = 9414 },
        new UpgradeStatAndCost(){ Value = 400, Cost = 12426 },
        new UpgradeStatAndCost(){ Value = 410, Cost = 16403 },
        new UpgradeStatAndCost(){ Value = 420, Cost = 21651 },
        new UpgradeStatAndCost(){ Value = 430, Cost = 28580 },
        new UpgradeStatAndCost(){ Value = 440, Cost = 37725 },
        new UpgradeStatAndCost(){ Value = 450, Cost = 49798 },
        new UpgradeStatAndCost(){ Value = 460, Cost = 65733 },
        new UpgradeStatAndCost(){ Value = 470, Cost = 86767 },
        new UpgradeStatAndCost(){ Value = 480, Cost = 114533 },
        new UpgradeStatAndCost(){ Value = 490, Cost = 151184 },
        new UpgradeStatAndCost(){ Value = 500, Cost = 199562 },
        new UpgradeStatAndCost(){ Value = 510, Cost = 263422 },
        new UpgradeStatAndCost(){ Value = 520, Cost = 347717 },
        new UpgradeStatAndCost(){ Value = 530, Cost = 458987 },
        new UpgradeStatAndCost(){ Value = 540, Cost = 605863 },
        new UpgradeStatAndCost(){ Value = 550, Cost = 799739 },
        new UpgradeStatAndCost(){ Value = 560, Cost = 1055655 },
        new UpgradeStatAndCost(){ Value = 570, Cost = 1393465 },
        new UpgradeStatAndCost(){ Value = 580, Cost = 1839374 },
        new UpgradeStatAndCost(){ Value = 590, Cost = 2427973 },
    };
}
