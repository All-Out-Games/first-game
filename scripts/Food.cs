using AO;

public class FoodDefinition
{
    public Texture Sprite;
    public String Name;
    public String Id;
    public long RequiredMouthSize;
    public double ConsumptionTime;
    public long NutritionValue;
}

public partial class Food : Component
{
    public static List<FoodDefinition> FoodDefinitions = new()
    {
        // new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/apple.png"), Name = "Apple", Id = "apple", RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5 }
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Apple 256.png"),            Name = "Apple",                Id = "Apple",                RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/broccoli.png"),             Name = "broccoli",             Id = "broccoli",             RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/HotDog.png"),               Name = "HotDog",               Id = "HotDog",               RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Underwear.png"),            Name = "Underwear",            Id = "Underwear",            RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Burger.png"),               Name = "Burger",               Id = "Burger",               RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Pizza.png"),                Name = "Pizza",                Id = "Pizza",                RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Popcorn.png"),              Name = "Popcorn",              Id = "Popcorn",              RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png"),           Name = "Watermelon",           Id = "Watermelon",           RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png"),  Name = "fire_hydrant_normal",  Id = "fire_hydrant_normal",  RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png"),          Name = "Potted_Tree",          Id = "Potted_Tree",          RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png"),  Name = "Picnick_Table_Empty",  Id = "Picnick_Table_Empty",  RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png"), Name = "Garbage_Bins_Unclean", Id = "Garbage_Bins_Unclean", RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/Car.png"),                  Name = "Car",                  Id = "Car",                  RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/tesla.png"),                Name = "tesla",                Id = "tesla",                RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/monster truck clean.png"),  Name = "monster",              Id = "monster",              RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
        new FoodDefinition(){Sprite = Assets.GetAsset<Texture>("food_items/plane_clean.png"),          Name = "plane_clean",          Id = "plane_clean",          RequiredMouthSize = 0, ConsumptionTime = 1, NutritionValue = 5},
    };

    public const string EatingFreezeReason = "EATING_FOOD";

    [Serialized] public int NutritionValue;
    [Serialized] public int ConsumptionTime;
    [Serialized] public int Size;

    public FatPlayer CurrentEater;
    public float EatingTime;

    public Action OnEat;

    public override void Start()
    {
        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract = (Player p) =>
        {
            var player = (FatPlayer) p;
            if (player.MouthSize < Size)
            {
                if (Network.IsClient)
                {
                    Notifications.Show("Your mouth is too small to eat this food!");
                }
                return;
            }

            var stomachRoom = player.ModifiedMaxFood - player.Food;
            if (stomachRoom <= 0)
            {
                if (Network.IsClient)
                {
                    Notifications.Show("You are too full to eat this food!");
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
                Log.Info("player.FoodBeingEaten");
                return false;
            }
            if (player.CurrentBoss != null)
            {
                Log.Info("player.CurrentBoss");
                return false;
            }
            return true;
        };
    }

    public override void Update()
    {
        if (CurrentEater == null) return;
        EatingTime += Time.DeltaTime * CurrentEater.ModifiedChewSpeed;

        if (Network.IsClient) return;
        if (EatingTime >= ConsumptionTime) {
            CallClient_FinishEating(true);
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
        if (Network.IsServer && success) {            
            OnEat?.Invoke();
            CurrentEater.Food += NutritionValue;
            Network.Despawn(this.Entity);
            this.Entity.Destroy();
        }
        
        CurrentEater.RemoveFreezeReason(EatingFreezeReason);
        CurrentEater.FoodBeingEaten = null;
        CurrentEater = null;
    }
}