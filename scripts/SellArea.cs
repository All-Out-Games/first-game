using AO;

public class SellArea : Component 
{
    public Interactable Interactable;

    public override void Start()
    {
        Interactable = Entity.GetComponent<Interactable>();
        Interactable.OnInteract += OnInteract;
    }

    public void OnInteract(Player player)
    {
        if (Network.IsClient) {
            return;
        }

        var fatPlayer = (FatPlayer) player;
        fatPlayer.Coins += fatPlayer.Food * Rebirth.Instance.GetRebirthData(fatPlayer.Rebirth).CashMultiplier;
        fatPlayer.Food = 0;
    }
}