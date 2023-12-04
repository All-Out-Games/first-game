using AO;

public class Boss : Component
{
    [Serialized] public string Name;
    [Serialized] public float Speed;

    public override void Start()
    {
        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract += OnInteract;
    }

    public void OnInteract(Player p)
    {
        var player = (FatPlayer) p;
        if (Network.IsServer) {
            Log.Info("Starting boss fight with " + player.Name);
            player.CallClient_StartBossFight(Entity.NetworkId);
        }
    }
}