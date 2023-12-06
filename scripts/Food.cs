using AO;

public partial class Food : Component
{
    public const string EatingFreezeReason = "EATING_FOOD";

    [Serialized] public int NutritionValue;
    [Serialized] public int ConsumptionTime;
    [Serialized] public int Size;

    public FatPlayer CurrentEater;
    public float EatingTime;

    public Interactable Interactable;
    public Action OnEat;

    public override void Start()
    {
        Interactable = Entity.GetComponent<Interactable>();
        Interactable.OnInteract += OnInteract;
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
    

    public void OnInteract(Player p)
    {
        var player = (FatPlayer) p;
        
        var stomachRoom = player.MaxFood - player.Food;
        if (stomachRoom < NutritionValue) {
            if (Network.IsClient) {
                Notifications.Show("You are too full to eat this food!");
            }
            return;
        }

        if (player.MouthSize < Size) {
            if (Network.IsClient) {
                Notifications.Show("Your mouth is too small to eat this food!");
            }
            return;
        }

        if (Network.IsServer) {
            CallClient_StartEating(p.Entity.NetworkId);
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