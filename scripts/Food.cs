using AO;

public class FoodDefinition
{
    public Texture Sprite;
    public String Name;
    public String Id;
    public double RequiredMouthSize;
    public double ConsumptionTime;
    public double SellValue;
    public double StomachSpace;
}

public partial class Food : Component
{
    public static List<FoodDefinition> FoodDefinitions = new()
    {
        new FoodDefinition(){Id = "Apple1",                Name = "Apple",                RequiredMouthSize = 1,  ConsumptionTime = 1,   StomachSpace = 1,   SellValue = 2,        Sprite = Assets.GetAsset<Texture>("food_items/Apple 256.png") },
        new FoodDefinition(){Id = "broccoli1",             Name = "broccoli",             RequiredMouthSize = 4,  ConsumptionTime = 2,   StomachSpace = 3,   SellValue = 6,        Sprite = Assets.GetAsset<Texture>("food_items/broccoli.png") },
        new FoodDefinition(){Id = "HotDog1",               Name = "HotDog",               RequiredMouthSize = 9,  ConsumptionTime = 3,   StomachSpace = 5,   SellValue = 12,       Sprite = Assets.GetAsset<Texture>("food_items/HotDog.png") },
        new FoodDefinition(){Id = "Underwear1",            Name = "Underwear",            RequiredMouthSize = 17, ConsumptionTime = 5,   StomachSpace = 8,   SellValue = 18,       Sprite = Assets.GetAsset<Texture>("food_items/Underwear.png") },
        new FoodDefinition(){Id = "Burger1",               Name = "Burger",               RequiredMouthSize = 29, ConsumptionTime = 9,   StomachSpace = 10,  SellValue = 24,       Sprite = Assets.GetAsset<Texture>("food_items/Burger.png") },
        new FoodDefinition(){Id = "Pizza1",                Name = "Pizza",                RequiredMouthSize = 43, ConsumptionTime = 15,  StomachSpace = 9,   SellValue = 30,       Sprite = Assets.GetAsset<Texture>("food_items/Pizza.png") },
        new FoodDefinition(){Id = "Popcorn1",              Name = "Popcorn",              RequiredMouthSize = 59, ConsumptionTime = 21,  StomachSpace = 6,   SellValue = 36,       Sprite = Assets.GetAsset<Texture>("food_items/Popcorn.png") },
        new FoodDefinition(){Id = "Watermelon1",           Name = "Watermelon",           RequiredMouthSize = 73, ConsumptionTime = 18,  StomachSpace = 21,  SellValue = 42,       Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal1",  Name = "fire_hydrant_normal",  RequiredMouthSize = 95, ConsumptionTime = 25,  StomachSpace = 29,  SellValue = 48,       Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },

        new FoodDefinition(){Id = "Underwear2",            Name = "Underwear",            RequiredMouthSize = 99,  ConsumptionTime = 12, StomachSpace = 12, SellValue = 314,       Sprite = Assets.GetAsset<Texture>("food_items/Underwear.png") },
        new FoodDefinition(){Id = "Burger2",               Name = "Burger",               RequiredMouthSize = 122, ConsumptionTime = 16, StomachSpace = 17, SellValue = 628,       Sprite = Assets.GetAsset<Texture>("food_items/Burger.png") },
        new FoodDefinition(){Id = "Pizza2",                Name = "Pizza",                RequiredMouthSize = 108, ConsumptionTime = 19, StomachSpace = 10, SellValue = 942,       Sprite = Assets.GetAsset<Texture>("food_items/Pizza.png") },
        new FoodDefinition(){Id = "Popcorn2",              Name = "Popcorn",              RequiredMouthSize = 145, ConsumptionTime = 18, StomachSpace = 31, SellValue = 1256,      Sprite = Assets.GetAsset<Texture>("food_items/Popcorn.png") },
        new FoodDefinition(){Id = "Watermelon2",           Name = "Watermelon",           RequiredMouthSize = 152, ConsumptionTime = 23, StomachSpace = 22, SellValue = 1570,      Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal2",  Name = "fire_hydrant_normal",  RequiredMouthSize = 169, ConsumptionTime = 26, StomachSpace = 29, SellValue = 1884,      Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },
        new FoodDefinition(){Id = "Potted_Tree2",          Name = "Potted_Tree",          RequiredMouthSize = 199, ConsumptionTime = 35, StomachSpace = 65, SellValue = 2198,      Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png") },
        new FoodDefinition(){Id = "Picnick_Table_Empty2",  Name = "Picnick_Table_Empty",  RequiredMouthSize = 227, ConsumptionTime = 29, StomachSpace = 31, SellValue = 2512,      Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png") },
        new FoodDefinition(){Id = "Garbage_Bins_Unclean2", Name = "Garbage_Bins_Unclean", RequiredMouthSize = 244, ConsumptionTime = 37, StomachSpace = 72, SellValue = 2826,      Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png") },

        new FoodDefinition(){Id = "Watermelon3",           Name = "Watermelon",           RequiredMouthSize = 250, ConsumptionTime = 18, StomachSpace = 35,  SellValue = 51700,    Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal3",  Name = "fire_hydrant_normal",  RequiredMouthSize = 302, ConsumptionTime = 26, StomachSpace = 40,  SellValue = 103400,   Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },
        new FoodDefinition(){Id = "Potted_Tree3",          Name = "Potted_Tree",          RequiredMouthSize = 430, ConsumptionTime = 44, StomachSpace = 43,  SellValue = 206800,   Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png") },
        new FoodDefinition(){Id = "Picnick_Table_Empty3",  Name = "Picnick_Table_Empty",  RequiredMouthSize = 333, ConsumptionTime = 22, StomachSpace = 23,  SellValue = 413600,   Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png") },
        new FoodDefinition(){Id = "Garbage_Bins_Unclean3", Name = "Garbage_Bins_Unclean", RequiredMouthSize = 377, ConsumptionTime = 37, StomachSpace = 41,  SellValue = 827200,   Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png") },
        new FoodDefinition(){Id = "Car3",                  Name = "Car",                  RequiredMouthSize = 420, ConsumptionTime = 39, StomachSpace = 47,  SellValue = 1654400,  Sprite = Assets.GetAsset<Texture>("food_items/Car.png") },
        new FoodDefinition(){Id = "tesla3",                Name = "tesla",                RequiredMouthSize = 404, ConsumptionTime = 45, StomachSpace = 51,  SellValue = 3308800,  Sprite = Assets.GetAsset<Texture>("food_items/tesla.png") },
        new FoodDefinition(){Id = "monster3",              Name = "monster",              RequiredMouthSize = 448, ConsumptionTime = 56, StomachSpace = 10,  SellValue = 6617600,  Sprite = Assets.GetAsset<Texture>("food_items/monster truck clean.png") },
        new FoodDefinition(){Id = "plane_clean3",          Name = "plane_clean",          RequiredMouthSize = 495, ConsumptionTime = 60, StomachSpace = 100, SellValue = 13235200, Sprite = Assets.GetAsset<Texture>("food_items/plane_clean.png") },
    };

    public const string EatingFreezeReason = "EATING_FOOD";

    [Serialized] public Sprite_Renderer SpriteRenderer;
    [Serialized] public string FoodId;

    public FoodDefinition Definition;

    public double Size            => Definition.RequiredMouthSize;
    public double ConsumptionTime => Definition.ConsumptionTime;
    public double SellValue       => Definition.SellValue;
    public double StomachSpace    => Definition.StomachSpace;

    public FatPlayer CurrentEater;
    public double EatingTime;

    public int FoodIndexInAreaDefinition; // only on server

    public event Action<Food> OnEat;

    public bool PlayerCanEatThis(FatPlayer player, out string reason, bool giveReason = false) // giveReason is for perf reasons when we dont need the reason
    {
        reason = string.Empty;
        if (player.ModifiedMouthSize < Size)
        {
            if (giveReason)
            {
                reason = $"Your mouth is too small to eat {Definition.Name}! {Util.FormatDouble(player.ModifiedMouthSize)}/{Util.FormatDouble(Size)}";
            }
            return false;
        }

        var stomachRoom = player.ModifiedStomachSize - player.AmountOfFoodInStomach;
        if (stomachRoom <= 0)
        {
            if (giveReason)
            {
                reason = "You are too full to eat this food!";
            }
            return false;
        }

        if (CurrentEater != null)
        {
            if (giveReason)
            {
                reason = "Somebody else is eating that!";
            }
            return false;
        }

        return true;
    }

    public override void Start()
    {
        Definition = FoodDefinitions.FirstOrDefault(f => f.Id == FoodId);
        if (Definition == null)
        {
            Log.Error($"Failed to find food for ID '{FoodId}'");
            return;
        }

        SpriteRenderer.Sprite = Definition.Sprite;
        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract = (Player p) =>
        {
            var player = (FatPlayer) p;
            if (!PlayerCanEatThis(player, out string reason, true))
            {
                if (player.IsLocal)
                {
                    Notifications.Show(reason);
                }
                return;
            }

            if (Network.IsServer)
            {
                CallClient_StartEating(p.Entity.NetworkId);
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
            if (CurrentEater != null)
            {
                return false;
            }
            return true;
        };
    }

    public override void Update()
    {
        var localPlayer = (FatPlayer) Network.LocalPlayer;
        if (localPlayer != null)
        {
            if (PlayerCanEatThis(localPlayer, out string reason))
            {
                SpriteRenderer.Tint = new Vector4(1, 1, 1, 1);
            }
            else
            {
                if (CurrentEater != localPlayer)
                {
                    SpriteRenderer.Tint = new Vector4(0.25f, 0.25f, 0.25f, 1);
                }
            }
        }

        if (CurrentEater != null)
        {
            EatingTime += (double)Time.DeltaTime * CurrentEater.ModifiedChewSpeed;

            if (Network.IsClient) return;
            if (EatingTime >= ConsumptionTime)
            {
                CallClient_FinishEating(true);
            }
        }
    }

    [ClientRpc]
    public void StartEating(ulong playerNetworkId)
    {
        var player = Entity.FindByNetworkId(playerNetworkId).GetComponent<FatPlayer>();
        if (player == null) return;
        
        CurrentEater = player;
        CurrentEater.AddFreezeReason(EatingFreezeReason);
        player.FoodBeingEaten = this;
        EatingTime = 0;
    }

    [ClientRpc]
    public void FinishEating(bool success)
    {
        if (Network.IsServer && success)
        {
            OnEat?.Invoke(this);
            CurrentEater.AmountOfFoodInStomach += StomachSpace;
            CurrentEater.ValueOfFoodInStomach += SellValue;
            if (CurrentEater.CurrentQuest != null)
            {
                CurrentEater.CurrentQuest.OnFoodEatenServer(this);
            }
            Network.Despawn(this.Entity);
            this.Entity.Destroy();
        }
        
        CurrentEater.RemoveFreezeReason(EatingFreezeReason);
        CurrentEater.FoodBeingEaten = null;
        CurrentEater = null;
    }
}