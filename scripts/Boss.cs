using AO;

public class Boss : Component
{
    public const float BOSS_TIMER = 10f;

    [Serialized] public string Name;
    [Serialized] public int Increment;
    [Serialized] public float Tick;
    [Serialized] public int Reward;
    [Serialized] public int AmountToWin;

    public override void Start()
    {
        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract += (Player p) =>
        {
            var player = (FatPlayer) p;
            if (Network.IsServer) {
                Log.Info("Starting boss fight with " + player.Name);
                player.CallClient_StartBossFight(Entity.NetworkId);
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
}