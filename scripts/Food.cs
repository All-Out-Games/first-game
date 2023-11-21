using AO;

public class Food : Component
{
    [Serialized] public int Value;

    public Interactable Interactable;

    public override void Start()
    {
        Interactable = Entity.GetComponent<Interactable>();
        Interactable.OnInteract += OnInteract;
    }

    public void OnInteract(Player player)
    {
        Network.Despawn(this.Entity);
        Log.Info("Interact with food!");
    }
}