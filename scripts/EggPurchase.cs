using AO;

public class EggPurchase : Component
{
    [Serialized] public string EggItemId;

    public override void Start()
    {
        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract += OnInteract;
    }

    public void OnInteract(Player p)
    {
        var player = (FatPlayer) p;

        var item = ShopData.Items.FirstOrDefault(x => x.Id == EggItemId);
        if (item == null)
        {
            Log.Error($"Player tried to purchase {EggItemId} but it doesn't exist.");
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
        Shop.Instance.Purchase(player, EggItemId);
    }
}