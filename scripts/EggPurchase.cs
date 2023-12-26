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

    public override void Update()
    {
        if (Network.IsServer) return;
        if (Network.LocalPlayer == null) return;

        var item = ShopData.Items.FirstOrDefault(x => x.Id == EggId);
        if (item == null)
        {
            return;
        }

        var localPlayer = (FatPlayer) Network.LocalPlayer;
        Interactable.Text = "Buy";

        if (Interactable.IsWorldUIShowing())
        {
            UI.PushContext(UI.Context.WORLD);
            using var _ = AllOut.Defer(UI.PopContext);

            var worldRect = Interactable.GetWorldRect();
            var rect = worldRect.TopCenterRect().Grow(0.2f, 0.3f, 0, 0.3f);
            UI.Image(rect, References.Instance.FrameDark, new AO.Vector4(0,0,0, 0.5f), References.Instance.FrameSlice);

            var textSettings = new UI.TextSettings()
            {
                color = Vector4.White,
                size = 0.175f,
                horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                outline = true,
                outlineThickness = 0.1f,
            };

            // localPlayer.ZoneCosts.TryGetValue(ZoneName, out var cost);
            UI.Image(rect.LeftRect().Grow(0, 0.1f, 0, 0.1f), References.Instance.CoinIcon, Vector4.White);
            UI.Text(rect, $"{item.Cost}", textSettings);
        }
    }
}