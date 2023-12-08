using AO;

public class FoodDefinition
{
    public Texture Sprite;
    public String Name;
    public String Id;
    public int RequiredMouthSize;
    public float ConsumptionTime;
    public int NutritionValue;
}

public partial class Food : Component
{
    public static List<FoodDefinition> FoodDefinitions = new()
    {
        new FoodDefinition(){Id = "Apple",                Name = "Apple",                RequiredMouthSize = 0, ConsumptionTime = 1,  NutritionValue = 7,   Sprite = Assets.GetAsset<Texture>("food_items/Apple 256.png") },
        new FoodDefinition(){Id = "broccoli",             Name = "broccoli",             RequiredMouthSize = 0, ConsumptionTime = 1,  NutritionValue = 7,   Sprite = Assets.GetAsset<Texture>("food_items/broccoli.png") },
        new FoodDefinition(){Id = "HotDog",               Name = "HotDog",               RequiredMouthSize = 0, ConsumptionTime = 1,  NutritionValue = 7,   Sprite = Assets.GetAsset<Texture>("food_items/HotDog.png") },
        new FoodDefinition(){Id = "Underwear",            Name = "Underwear",            RequiredMouthSize = 0, ConsumptionTime = 1,  NutritionValue = 7,   Sprite = Assets.GetAsset<Texture>("food_items/Underwear.png") },
        new FoodDefinition(){Id = "Burger",               Name = "Burger",               RequiredMouthSize = 0, ConsumptionTime = 2,  NutritionValue = 15,  Sprite = Assets.GetAsset<Texture>("food_items/Burger.png") },
        new FoodDefinition(){Id = "Pizza",                Name = "Pizza",                RequiredMouthSize = 0, ConsumptionTime = 2,  NutritionValue = 15,  Sprite = Assets.GetAsset<Texture>("food_items/Pizza.png") },
        new FoodDefinition(){Id = "Popcorn",              Name = "Popcorn",              RequiredMouthSize = 0, ConsumptionTime = 2,  NutritionValue = 15,  Sprite = Assets.GetAsset<Texture>("food_items/Popcorn.png") },
        new FoodDefinition(){Id = "Watermelon",           Name = "Watermelon",           RequiredMouthSize = 0, ConsumptionTime = 3,  NutritionValue = 22,  Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal",  Name = "fire_hydrant_normal",  RequiredMouthSize = 0, ConsumptionTime = 5,  NutritionValue = 37,  Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },
        new FoodDefinition(){Id = "Potted_Tree",          Name = "Potted_Tree",          RequiredMouthSize = 0, ConsumptionTime = 5,  NutritionValue = 37,  Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png") },
        new FoodDefinition(){Id = "Picnick_Table_Empty",  Name = "Picnick_Table_Empty",  RequiredMouthSize = 0, ConsumptionTime = 6,  NutritionValue = 45,  Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png") },
        new FoodDefinition(){Id = "Garbage_Bins_Unclean", Name = "Garbage_Bins_Unclean", RequiredMouthSize = 0, ConsumptionTime = 8,  NutritionValue = 60,  Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png") },
        new FoodDefinition(){Id = "Car",                  Name = "Car",                  RequiredMouthSize = 0, ConsumptionTime = 10, NutritionValue = 75,  Sprite = Assets.GetAsset<Texture>("food_items/Car.png") },
        new FoodDefinition(){Id = "tesla",                Name = "tesla",                RequiredMouthSize = 0, ConsumptionTime = 10, NutritionValue = 75,  Sprite = Assets.GetAsset<Texture>("food_items/tesla.png") },
        new FoodDefinition(){Id = "monster",              Name = "monster",              RequiredMouthSize = 0, ConsumptionTime = 20, NutritionValue = 150, Sprite = Assets.GetAsset<Texture>("food_items/monster truck clean.png") },
        new FoodDefinition(){Id = "plane_clean",          Name = "plane_clean",          RequiredMouthSize = 0, ConsumptionTime = 30, NutritionValue = 225, Sprite = Assets.GetAsset<Texture>("food_items/plane_clean.png") },
    };

    public const string EatingFreezeReason = "EATING_FOOD";

    [Serialized] public Sprite_Renderer SpriteRenderer;
    [Serialized] public string FoodId;

    public FoodDefinition Definition;

    public int   Size            => Definition.RequiredMouthSize;
    public float ConsumptionTime => Definition.ConsumptionTime;
    public int   NutritionValue  => Definition.NutritionValue;

    public FatPlayer CurrentEater;
    public float EatingTime;

    public Action OnEat;

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