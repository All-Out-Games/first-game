using AO;

public partial class FatPlayer : Player 
{
    private int _trophies;
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

    private int _coins;
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

    private int _food;
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

    private int _maxFood;
    public int MaxFood 
    { 
        get => _maxFood; 
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

    private int _mouthSize;
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

    private int _chewSpeed;
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


    public override void Start()
    {
        
    }

    public void OnLoad()
    {
        if (Network.IsServer)
        {
            Trophies = Save.GetInt(this, "Trophies", 0);
            Coins = Save.GetInt(this, "Coins", 0);
            Food = Save.GetInt(this, "Food", 0);
            MaxFood = Save.GetInt(this, "MaxFood", 0);
            MouthSize = Save.GetInt(this, "MouthSize", 0);
            ChewSpeed = Save.GetInt(this, "ChewSpeed", 0);
        }
    }

    [ClientRpc] public void NotifyTrophiesUpdate(int val)  { if (Network.IsClient) Trophies = val;  }
    [ClientRpc] public void NotifyCoinsUpdate(int val)     { if (Network.IsClient) Coins = val;     }
    [ClientRpc] public void NotifyFoodUpdate(int val)      { if (Network.IsClient) { Food = val; Log.Info("Updating food via RPC"); }     }
    [ClientRpc] public void NotifyMaxFoodUpdate(int val)   { if (Network.IsClient) MaxFood = val;   }
    [ClientRpc] public void NotifyMouthSizeUpdate(int val) { if (Network.IsClient) MouthSize = val; }
    [ClientRpc] public void NotifyChewSpeedUpdate(int val) { if (Network.IsClient) ChewSpeed = val; }
}