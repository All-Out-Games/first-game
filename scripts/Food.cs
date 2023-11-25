using AO;

public class Food : Component
{
    [Serialized] public int Value;

    public Interactable Interactable;
    public Action OnEat;

    public override void Start()
    {
        Interactable = Entity.GetComponent<Interactable>();
        Interactable.OnInteract += OnInteract;
    }

    public void OnInteract(Player p)
    {
        if (Network.IsClient) {
            Log.Info("Interact with food!");
            return;
        }

        OnEat?.Invoke();
        
        var player = (FatPlayer) p;
        player.Food += Value;
        Network.Despawn(this.Entity);
        this.Entity.Destroy();

        Log.Info("Interact with food!");
    }
}