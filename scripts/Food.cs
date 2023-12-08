using AO;

public class FoodDefinition
{
    public Texture Sprite;
    public String Name;
    public String Id;
    public double RequiredMouthSize;
    public double ConsumptionTime;
    public double NutritionValue;
}

public partial class Food : Component
{
    public static List<FoodDefinition> FoodDefinitions = new()
    {
        new FoodDefinition(){Id = "Apple1",                Name = "Apple",                RequiredMouthSize = 1,  ConsumptionTime = 1,     NutritionValue = 1,   Sprite = Assets.GetAsset<Texture>("food_items/Apple 256.png") },
        new FoodDefinition(){Id = "broccoli1",             Name = "broccoli",             RequiredMouthSize = 1,  ConsumptionTime = 5,     NutritionValue = 6,   Sprite = Assets.GetAsset<Texture>("food_items/broccoli.png") },
        new FoodDefinition(){Id = "HotDog1",               Name = "HotDog",               RequiredMouthSize = 5,  ConsumptionTime = 5,     NutritionValue = 12,  Sprite = Assets.GetAsset<Texture>("food_items/HotDog.png") },
        new FoodDefinition(){Id = "Underwear1",            Name = "Underwear",            RequiredMouthSize = 5,  ConsumptionTime = 10,    NutritionValue = 18,  Sprite = Assets.GetAsset<Texture>("food_items/Underwear.png") },
        new FoodDefinition(){Id = "Burger1",               Name = "Burger",               RequiredMouthSize = 17, ConsumptionTime = 30,    NutritionValue = 24,  Sprite = Assets.GetAsset<Texture>("food_items/Burger.png") },
        new FoodDefinition(){Id = "Pizza1",                Name = "Pizza",                RequiredMouthSize = 24, ConsumptionTime = 15,    NutritionValue = 30,  Sprite = Assets.GetAsset<Texture>("food_items/Pizza.png") },
        new FoodDefinition(){Id = "Popcorn1",              Name = "Popcorn",              RequiredMouthSize = 52, ConsumptionTime = 1*60,  NutritionValue = 36,  Sprite = Assets.GetAsset<Texture>("food_items/Popcorn.png") },
        new FoodDefinition(){Id = "Watermelon1",           Name = "Watermelon",           RequiredMouthSize = 73, ConsumptionTime = 2*60,  NutritionValue = 42,  Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal1",  Name = "fire_hydrant_normal",  RequiredMouthSize = 95, ConsumptionTime = 5*60,  NutritionValue = 48,  Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },

        new FoodDefinition(){Id = "Underwear2",            Name = "Underwear",            RequiredMouthSize = 99,  ConsumptionTime = 1  * 60,  NutritionValue = 314,  Sprite = Assets.GetAsset<Texture>("food_items/Underwear.png") },
        new FoodDefinition(){Id = "Burger2",               Name = "Burger",               RequiredMouthSize = 122, ConsumptionTime = 3  * 60,  NutritionValue = 628,  Sprite = Assets.GetAsset<Texture>("food_items/Burger.png") },
        new FoodDefinition(){Id = "Pizza2",                Name = "Pizza",                RequiredMouthSize = 108, ConsumptionTime = 10 * 60,  NutritionValue = 942,  Sprite = Assets.GetAsset<Texture>("food_items/Pizza.png") },
        new FoodDefinition(){Id = "Popcorn2",              Name = "Popcorn",              RequiredMouthSize = 145, ConsumptionTime = 12 * 60,  NutritionValue = 1256, Sprite = Assets.GetAsset<Texture>("food_items/Popcorn.png") },
        new FoodDefinition(){Id = "Watermelon2",           Name = "Watermelon",           RequiredMouthSize = 152, ConsumptionTime = 17 * 60,  NutritionValue = 1570, Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal2",  Name = "fire_hydrant_normal",  RequiredMouthSize = 169, ConsumptionTime = 21 * 60,  NutritionValue = 1884, Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },
        new FoodDefinition(){Id = "Potted_Tree2",          Name = "Potted_Tree",          RequiredMouthSize = 199, ConsumptionTime = 19 * 60,  NutritionValue = 2198, Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png") },
        new FoodDefinition(){Id = "Picnick_Table_Empty2",  Name = "Picnick_Table_Empty",  RequiredMouthSize = 227, ConsumptionTime = 30 * 60,  NutritionValue = 2512, Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png") },
        new FoodDefinition(){Id = "Garbage_Bins_Unclean2", Name = "Garbage_Bins_Unclean", RequiredMouthSize = 244, ConsumptionTime = 14 * 60,  NutritionValue = 2826, Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png") },

        new FoodDefinition(){Id = "Watermelon3",           Name = "Watermelon",           RequiredMouthSize = 250, ConsumptionTime = 5  * 60,     NutritionValue = 51700,    Sprite = Assets.GetAsset<Texture>("food_items/Watermelon.png") },
        new FoodDefinition(){Id = "fire_hydrant_normal3",  Name = "fire_hydrant_normal",  RequiredMouthSize = 302, ConsumptionTime = 13 * 60,     NutritionValue = 103400,   Sprite = Assets.GetAsset<Texture>("food_items/fire_hydrant_normal.png") },
        new FoodDefinition(){Id = "Potted_Tree3",          Name = "Potted_Tree",          RequiredMouthSize = 430, ConsumptionTime = 24 * 60,     NutritionValue = 206800,   Sprite = Assets.GetAsset<Texture>("food_items/Potted_Tree.png") },
        new FoodDefinition(){Id = "Picnick_Table_Empty3",  Name = "Picnick_Table_Empty",  RequiredMouthSize = 333, ConsumptionTime = 19 * 60,     NutritionValue = 413600,   Sprite = Assets.GetAsset<Texture>("food_items/Picnick_Table_Empty.png") },
        new FoodDefinition(){Id = "Garbage_Bins_Unclean3", Name = "Garbage_Bins_Unclean", RequiredMouthSize = 377, ConsumptionTime = 32 * 60,     NutritionValue = 827200,   Sprite = Assets.GetAsset<Texture>("food_items/Garbage_Bins_Unclean.png") },
        new FoodDefinition(){Id = "Car3",                  Name = "Car",                  RequiredMouthSize = 420, ConsumptionTime = 45 * 60,     NutritionValue = 1654400,  Sprite = Assets.GetAsset<Texture>("food_items/Car.png") },
        new FoodDefinition(){Id = "tesla3",                Name = "tesla",                RequiredMouthSize = 404, ConsumptionTime = 36 * 60,     NutritionValue = 3308800,  Sprite = Assets.GetAsset<Texture>("food_items/tesla.png") },
        new FoodDefinition(){Id = "monster3",              Name = "monster",              RequiredMouthSize = 448, ConsumptionTime = 1 * 60 * 60, NutritionValue = 6617600,  Sprite = Assets.GetAsset<Texture>("food_items/monster truck clean.png") },
        new FoodDefinition(){Id = "plane_clean3",          Name = "plane_clean",          RequiredMouthSize = 495, ConsumptionTime = 2 * 60 * 60, NutritionValue = 13235200, Sprite = Assets.GetAsset<Texture>("food_items/plane_clean.png") },
    };

    public const string EatingFreezeReason = "EATING_FOOD";

    [Serialized] public Sprite_Renderer SpriteRenderer;
    [Serialized] public string FoodId;

    public FoodDefinition Definition;

    public double Size            => Definition.RequiredMouthSize;
    public double ConsumptionTime => Definition.ConsumptionTime;
    public double NutritionValue  => Definition.NutritionValue;

    public FatPlayer CurrentEater;
    public double EatingTime;

    public int FoodIndexInAreaDefinition; // only on server

    public event Action<Food> OnEat;

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
            if (player.ModifiedMouthSize < Size)
            {
                if (Network.IsClient)
                {
                    Notifications.Show($"Your mouth is too small to eat {Definition.Name}! {player.ModifiedMouthSize}/{Size}");
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
        EatingTime += (double)Time.DeltaTime * CurrentEater.ModifiedChewSpeed;

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
            OnEat?.Invoke(this);
            CurrentEater.Food += NutritionValue;
            Network.Despawn(this.Entity);
            this.Entity.Destroy();
        }
        
        CurrentEater.RemoveFreezeReason(EatingFreezeReason);
        CurrentEater.FoodBeingEaten = null;
        CurrentEater = null;
    }
}