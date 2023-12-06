using AO;

public partial class FatPlayer : Player 
{
    public const int MinStomachSize = 10;

    private int _trophies;
    private int _coins;
    private int _food;
    private int _maxFood;
    private int _mouthSize;
    private int _chewSpeed;
    private int _rebirth;

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
    public float BossTimer;
    public float BossAccumulator;
    public int BossProgress;
    public int MyProgress;

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
            if (Network.IsClient)
                Notifications.Show("You won the boss fight!");
        }
        else 
        {
            if (Network.IsClient)
                Notifications.Show("You lost the boss fight!");
        }


        CurrentBoss = null;
        this.RemoveFreezeReason("BossFight");
    }

    public override void Update()
    {
        if (CurrentBoss != null)
        {
            if (this.IsMouseUpLeft()) 
            {
                MyProgress += (ChewSpeed + MouthSize) / 2;
            }

            BossAccumulator += Time.DeltaTime;
            if (BossAccumulator >= CurrentBoss.Tick) 
            {
                BossProgress += CurrentBoss.Increment;
                BossAccumulator = 0;
            }

            BossTimer -= Time.DeltaTime;
            if (Network.IsServer && BossTimer <= 0) 
            {
                CallClient_BossFightOver(MyProgress > BossProgress);
            }
        }

        if (FoodBeingEaten != null)
        {
            var chewRect = UI.GetPlayerRect(this);
            chewRect = chewRect.Grow(50, 50, 0, 50).Offset(0, 10);

            UI.Image(chewRect, null, Vector4.White, new UI.NineSlice());

            var chewProgress = FoodBeingEaten.EatingTime / FoodBeingEaten.ConsumptionTime;
            var chewProgressRect = chewRect.SubRect(0, 0, chewProgress, 1, 0, 0, 0, 0);
            UI.Image(chewProgressRect, null, Vector4.HSVLerp(Vector4.Red, Vector4.Green, chewProgress), new UI.NineSlice());
        }

        if (this.IsLocal) 
        {
            if (FoodBeingEaten != null && Input.GetKeyDown(Input.Keycode.KEYCODE_ESCAPE, true)) {
                FoodBeingEaten.CallClient_FinishEating(false);
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
            MaxFood = Save.GetInt(this, "MaxFood", 10);
            MouthSize = Math.Max(1, Save.GetInt(this, "MouthSize", 1));
            ChewSpeed = Math.Max(1, Save.GetInt(this, "ChewSpeed", 1));
            Rebirth = Save.GetInt(this, "Rebirth", 0);

            var pets = Save.GetString(this, "AllPets", "[]");
            CallClient_LoadPetData(pets);
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
        BossTimer = Boss.BOSS_TIMER;
        BossAccumulator = 0;
    }

    public int Trophies 
    { 
        get => _trophies; 
        set 
        { 
            _trophies = value; 
            if (Network.IsServer) 
            {
                Save.SetInt(this, "Trophies", value); 
                CallClient_NotifyTrophiesUpdate(value);
            }
        } 
    }

    public int Coins 
    { 
        get => _coins; 
        set 
        { 
            _coins = value; 
            if (Network.IsServer) 
            {
                Save.SetInt(this, "Coins", value); 
                CallClient_NotifyCoinsUpdate(value);
            }
        } 
    }

    public int Food 
    { 
        get => _food; 
        set 
        { 
            _food = value; 
            if (Network.IsServer) 
            {
                Save.SetInt(this, "Food", value); 
                CallClient_NotifyFoodUpdate(value);
            }
        } 
    }

    public int MaxFood 
    { 
        get => Math.Max(MinStomachSize, _maxFood); 
        set 
        { 
            _maxFood = value; 
            if (Network.IsServer) 
            {
                Save.SetInt(this, "MaxFood", value); 
                CallClient_NotifyMaxFoodUpdate(value);
            }
        } 
    }

    public int MouthSize 
    { 
        get => _mouthSize; 
        set 
        { 
            _mouthSize = value; 
            if (Network.IsServer) 
            {
                Save.SetInt(this, "MouthSize", value); 
                CallClient_NotifyMouthSizeUpdate(value);
            }
        } 
    }

    public int ChewSpeed 
    { 
        get => _chewSpeed; 
        set 
        { 
            _chewSpeed = value; 
            if (Network.IsServer) 
            {
                Save.SetInt(this, "ChewSpeed", value); 
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
        this.ChewSpeed = 1;
        this.MouthSize = 1;
        this.MaxFood = 10;
        
        CallClient_DoRebirth(this.Rebirth);
    }

    [ClientRpc] public void NotifyTrophiesUpdate(int val)  { if (Network.IsClient) Trophies = val;  }
    [ClientRpc] public void NotifyCoinsUpdate(int val)     { if (Network.IsClient) Coins = val;     }
    [ClientRpc] public void NotifyFoodUpdate(int val)      { if (Network.IsClient) { Food = val; Log.Info("Updating food via RPC"); }     }
    [ClientRpc] public void NotifyMaxFoodUpdate(int val)   { if (Network.IsClient) MaxFood = val;   }
    [ClientRpc] public void NotifyMouthSizeUpdate(int val) { if (Network.IsClient) MouthSize = val; }
    [ClientRpc] public void NotifyChewSpeedUpdate(int val) { if (Network.IsClient) ChewSpeed = val; }

    [ServerRpc]
    public void RequestPurchaseStoachSize()
    {
        if (Network.IsClient) return;
        if (Coins < 10) return;

        Coins -= 10;
        MaxFood += 1;
    }

    [ServerRpc]
    public void RequestPurchaseMouthSize()
    {
        if (Network.IsClient) return;
        if (Coins < 10) return;

        Coins -= 10;
        MouthSize += 1;
    }

    [ServerRpc]
    public void RequestPurchaseChewSpeed()
    {
        if (Network.IsClient) return;
        if (Coins < 10) return;

        Coins -= 10;
        ChewSpeed += 1;
    }

    [ServerRpc]
    public void RequestPurchaseItem(string id)
    {
        Shop.Instance.Purchase(this, id);
    }
}