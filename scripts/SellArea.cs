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
        fatPlayer.Coins += fatPlayer.ValueOfFoodInStomach * Rebirth.Instance.GetRebirthData(fatPlayer.Rebirth).CashMultiplier;
        fatPlayer.ValueOfFoodInStomach = 0;
        fatPlayer.AmountOfFoodInStomach = 0;
        if (fatPlayer.CurrentQuest != null)
        {
            fatPlayer.CurrentQuest.OnSoldItemsServer();
        }
    }
}