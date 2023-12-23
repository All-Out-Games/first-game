using AO;

public class EggPurchase : Component
{
    [Serialized] public string EggId;
    [Serialized] public Interactable Interactable;

    public override void Start()
    {
        Interactable.OnInteract += OnInteract;
    }

    public void OnInteract(Player p)
    {
        var player = (FatPlayer) p;

        var item = ShopData.Items.FirstOrDefault(x => x.Id == EggId);
        if (item == null)
        {
            Log.Error($"Player tried to purchase {EggId} but it doesn't exist.");
            return;
        }

        if (item.Currency == ShopData.Currency.Sparks)
        {
            if (Network.IsClient)
            {
                Purchasing.PromptPurchase(item.ProductId);
            }
            return;
        }

        // Purchase can run on the client and server. Running on the client just allows it
        // to show notifications if the user cannot afford something
        Shop.Instance.Purchase(player, EggId);
    }
}