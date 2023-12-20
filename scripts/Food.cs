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
        new FoodDefinition(){Id = "apple",               Name = "Apple",                   ConsumptionTime = 1,  RequiredMouthSize = 1,   StomachSpace = 1,    SellValue = 2,  Sprite = Assets.GetAsset<Texture>("food_items/apple.png") },
        new FoodDefinition(){Id = "feastable_bar",       Name = "Feastable Bar",           ConsumptionTime = 2,  RequiredMouthSize = 5,   StomachSpace = 2,    SellValue = 4,  Sprite = Assets.GetAsset<Texture>("food_items/feastable_bar.png") },
        new FoodDefinition(){Id = "PB&J_Sandwich",       Name = "PB&J Sandwich",           ConsumptionTime = 2,  RequiredMouthSize = 10,  StomachSpace = 5,    SellValue = 8,  Sprite = Assets.GetAsset<Texture>("food_items/PB&J_Sandwich.png") },
        new FoodDefinition(){Id = "popcorn",             Name = "Popcorn",                 ConsumptionTime = 3,  RequiredMouthSize = 15,  StomachSpace = 7,    SellValue = 12, Sprite = Assets.GetAsset<Texture>("food_items/popcorn.png") },
        new FoodDefinition(){Id = "grimace_shake_small", Name = "Grimace Shake",           ConsumptionTime = 5,  RequiredMouthSize = 25,  StomachSpace = 9,    SellValue = 18, Sprite = Assets.GetAsset<Texture>("food_items/grimace_shake_small.png") },
        new FoodDefinition(){Id = "milk_jug",            Name = "Milk Jug",                ConsumptionTime = 7,  RequiredMouthSize = 35,  StomachSpace = 13,   SellValue = 24, Sprite = Assets.GetAsset<Texture>("food_items/milk_jug.png") },
        new FoodDefinition(){Id = "doggy_poop_bin",      Name = "Dog Poop",                ConsumptionTime = 9,  RequiredMouthSize = 45,  StomachSpace = 17,   SellValue = 32, Sprite = Assets.GetAsset<Texture>("food_items/doggy_poop_bin.png") },
        new FoodDefinition(){Id = "fire_hydrant",        Name = "Fire Hydrant",            ConsumptionTime = 11, RequiredMouthSize = 60,  StomachSpace = 21,   SellValue = 40, Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant.png") },
        new FoodDefinition(){Id = "trash_bag",           Name = "Trash Bag",               ConsumptionTime = 13, RequiredMouthSize = 75,  StomachSpace = 25,   SellValue = 50, Sprite = Assets.GetAsset<Texture>("food_items/trash_bag.png") },
        new FoodDefinition(){Id = "infinity_gauntlet",   Name = "Infinity Gauntlet",       ConsumptionTime = 15, RequiredMouthSize = 95,  StomachSpace = 29,   SellValue = 62, Sprite = Assets.GetAsset<Texture>("food_items/infinity_gauntlet.png") },

        new FoodDefinition(){Id = "Underwear2",            Name = "Underwear",             ConsumptionTime = 12, RequiredMouthSize = 99,  StomachSpace = 12,   SellValue = 314,       Sprite = Assets.GetAsset<Texture>("food_items/Underwear.png") },
        new FoodDefinition(){Id = "Burger2",               Name = "Burger",                ConsumptionTime = 16, RequiredMouthSize = 122, StomachSpace = 17,   SellValue = 628,       Sprite = Assets.GetAsset<Texture>("food_items/Burger.png") },
        new FoodDefinition(){Id = "Pizza2",                Name = "Pizza",                 ConsumptionTime = 19, RequiredMouthSize = 108, StomachSpace = 10,   SellValue = 942,       Sprite = Assets.GetAsset<Texture>("food_items/Pizza.png") },
        new FoodDefinition(){Id = "Popcorn2",              Name = "Popcorn",               ConsumptionTime = 18, RequiredMouthSize = 145, StomachSpace = 31,   SellValue = 1256,      Sprite = Assets.GetAsset<Texture>("food_items/popcorn.png") },
        new FoodDefinition(){Id = "Watermelon2",           Name = "Watermelon",            ConsumptionTime = 23, RequiredMouthSize = 152, StomachSpace = 22,   SellValue = 1570,      Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal2",  Name = "fire_hydrant_normal",   ConsumptionTime = 26, RequiredMouthSize = 169, StomachSpace = 29,   SellValue = 1884,      Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },
        new FoodDefinition(){Id = "Potted_Tree2",          Name = "Potted_Tree",           ConsumptionTime = 35, RequiredMouthSize = 199, StomachSpace = 65,   SellValue = 2198,      Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png") },
        new FoodDefinition(){Id = "Picnick_Table_Empty2",  Name = "Picnick_Table_Empty",   ConsumptionTime = 29, RequiredMouthSize = 227, StomachSpace = 31,   SellValue = 2512,      Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png") },
        new FoodDefinition(){Id = "Garbage_Bins_Unclean2", Name = "Garbage_Bins_Unclean",  ConsumptionTime = 37, RequiredMouthSize = 244, StomachSpace = 72,   SellValue = 2826,      Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png") },

        new FoodDefinition(){Id = "Watermelon3",           Name = "Watermelon",            ConsumptionTime = 18, RequiredMouthSize = 250, StomachSpace = 35,   SellValue = 51700,    Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal3",  Name = "fire_hydrant_normal",   ConsumptionTime = 26, RequiredMouthSize = 302, StomachSpace = 40,   SellValue = 103400,   Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },
        new FoodDefinition(){Id = "Potted_Tree3",          Name = "Potted_Tree",           ConsumptionTime = 44, RequiredMouthSize = 430, StomachSpace = 43,   SellValue = 206800,   Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png") },
        new FoodDefinition(){Id = "Picnick_Table_Empty3",  Name = "Picnick_Table_Empty",   ConsumptionTime = 22, RequiredMouthSize = 333, StomachSpace = 23,   SellValue = 413600,   Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png") },
        new FoodDefinition(){Id = "Garbage_Bins_Unclean3", Name = "Garbage_Bins_Unclean",  ConsumptionTime = 37, RequiredMouthSize = 377, StomachSpace = 41,   SellValue = 827200,   Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png") },
        new FoodDefinition(){Id = "Car3",                  Name = "Car",                   ConsumptionTime = 39, RequiredMouthSize = 420, StomachSpace = 47,   SellValue = 1654400,  Sprite = Assets.GetAsset<Texture>("food_items/Car.png") },
        new FoodDefinition(){Id = "tesla3",                Name = "tesla",                 ConsumptionTime = 45, RequiredMouthSize = 404, StomachSpace = 51,   SellValue = 3308800,  Sprite = Assets.GetAsset<Texture>("food_items/tesla.png") },
        new FoodDefinition(){Id = "monster3",              Name = "monster",               ConsumptionTime = 56, RequiredMouthSize = 448, StomachSpace = 10,   SellValue = 6617600,  Sprite = Assets.GetAsset<Texture>("food_items/monster truck clean.png") },
        new FoodDefinition(){Id = "plane_clean3",          Name = "plane_clean",           ConsumptionTime = 60, RequiredMouthSize = 495, StomachSpace = 100,  SellValue = 13235200, Sprite = Assets.GetAsset<Texture>("food_items/plane_clean.png") },
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

        Log.Info($"FoodId: {FoodId}");
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
            // EatingTime += (double)Time.DeltaTime * 0.2f;

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
        EatingTime = player.FoodProgressPerClick;
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