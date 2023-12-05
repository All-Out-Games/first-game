using AO;

public class Shop : System<Shop>
{
    public Random Random;

    public override void Start()
    {
        Purchasing.SetPurchaseHandler(OnPurchase);
        Random = new Random();
    }

    public override void Update()
    {
    }

    public string SelectedCategory = "Eggs";

    public bool DrawShop()
    {
        if (UI.Button(UI.ScreenRect, "", new UI.ButtonSettings(), new UI.TextSettings()).clicked) 
        {
            return false;
        }

        var buttonSettings = new UI.ButtonSettings();
        buttonSettings.color = Vector4.White;
        buttonSettings.clickedColor = Vector4.White * 0.7f;
        buttonSettings.hoverColor = Vector4.White * 0.9f;
        buttonSettings.pressedColor = Vector4.White * 0.5f;
        buttonSettings.sprite = References.Instance.GreenButton;
        buttonSettings.slice = new UI.NineSlice() { slice = new Vector4(12, 15, 48, 48), sliceScale = 1f };

        var buttonTextSettings = new UI.TextSettings() 
        {
            font = UI.TextSettings.Font.AlphaKind,
            size = 48,
            color = Vector4.Black,
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Right,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
            wordWrap = false,
            wordWrapOffset = 0,
            offset = new Vector2(0, 10),
            // dropShadow = false,
            // outline= false,
        };

        var windowRect = UI.SafeRect.CenterRect();
        windowRect = windowRect.Grow(200, 300, 200, 300);
        UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});

        var shopCategories = ShopData.ShopEntries.Select(s => s.Category).Distinct().ToList();

        var topBarRect = windowRect.CutTop(75).Offset(0, 25);
        foreach(var category in shopCategories) 
        {
            var categoryButtonRect = topBarRect.CutLeft(150).Inset(0, 5, 0, 5);
            var categoryButtonResult = UI.Button(categoryButtonRect, category, buttonSettings, buttonTextSettings);
            if (categoryButtonResult.clicked) 
            {
                SelectedCategory = category;
            }
        }

        FatPlayer localPlayer = (FatPlayer) Network.LocalPlayer;

        var shopItems = ShopData.ShopEntries.Where(s => s.Category == SelectedCategory).ToList();
        var grid = UI.GridLayout.Make(windowRect, 3, 2, UI.GridLayout.SizeSource.GRID_SIZE);
        foreach(var shopEntry in shopItems) 
        {
            var item = ShopData.Items.First(i => i.Id == shopEntry.ItemId);

            var itemButtonRect = grid.Next().Inset(5, 5, 5, 5);
            var itemButtonResult = UI.Button(itemButtonRect, shopEntry.ItemId, buttonSettings, buttonTextSettings);
            if (itemButtonResult.clicked) 
            {
                if (item.Currency == ShopData.Currency.Coins || 
                    item.Currency == ShopData.Currency.Trophies) 
                {
                    localPlayer.CallServer_RequestPurchaseItem(item.Id);
                }

                if (item.Currency == ShopData.Currency.Sparks) 
                {
                    Purchasing.PromptPurchase(item.ProductId);
                }
            }
        }
        
        return true;
    }

    public void Purchase(FatPlayer player, string itemId)
    {
        if (Network.IsClient)
        {
            Log.Error("You cannot purchase items on the client!");
            return;
        }

        var item = ShopData.Items.First(i => i.Id == itemId);

        if (item.Currency == ShopData.Currency.Coins) 
        {
            if (player.Coins < item.Cost)
            {
                Notifications.Show("You don't have enough coins!");
                return;
            }
        }

        if (item.Currency == ShopData.Currency.Trophies) 
        {
            if (player.Trophies < item.Cost)
            {
                Notifications.Show("You don't have enough trophies!");
                return;
            }
        }

        if (item.Currency == ShopData.Currency.Sparks) 
        {
            Log.Error("Tried to purchase sparks item. This must be done through Purchasing.PromptPurchase");
            return;
        }

        // We get here if the item was purchased with coins or trophies and they could afford it

        if (item.Kind == ShopData.ItemKind.Egg) 
        {
            if (!PetData.Eggs.TryGetValue(item.ItemIdentifier, out var eggDefinition))
            {
                Log.Error($"Could not find egg definition for {item.ItemIdentifier}");
                return;
            }

            var totalWeight = eggDefinition.PossiblePets.Sum(p => p.Weight);
            var rnd = Random.Next(0, totalWeight);

            PetData.WeightedPet selectedPet = null;
            foreach(var pet in eggDefinition.PossiblePets) 
            {
                if (rnd < pet.Weight) 
                {
                    selectedPet = pet;
                    break;
                }

                rnd -= pet.Weight;
            }

            if (selectedPet == null) 
            {
                Log.Error($"Could not select pet from egg {item.ItemIdentifier}");
                return;
            }

            Guid guid = Guid.NewGuid();
            player.CallClient_AddPet(guid.ToString(), selectedPet.Id);
        }

        if (item.Currency == ShopData.Currency.Coins) 
        {
            player.Coins -= item.Cost;
        }

        if (item.Currency == ShopData.Currency.Trophies) 
        {
            player.Trophies -= item.Cost;
        }
    }

    public bool OnPurchase(Player purchaser, string productId)
    {
        return false;
    }
}

public static class ShopData
{
    [AOIgnore] public static List<ShopEntry> ShopEntries = new List<ShopEntry>() 
    {
        new () { Id = "fun_egg_item", Category = "Eggs", ItemId = "fun_egg" },
    };

    [AOIgnore] public static List<Item> Items = new List<Item>() 
    {
        new () { Id = "fun_egg", ProductId = "", Name = "Fun Egg", Description = "A fun egg", Currency = Currency.Coins, Cost = 100, Kind = ItemKind.Egg, ItemIdentifier = "egg0" },
    };

    public class ShopEntry
    {
        public string Id;
        public string Category;
        public string ItemId;
    }


    public enum ItemKind
    {
        Egg,
        Pet,
        Boost,
        Pass,
    }

    public enum Currency
    {
        Coins,
        Trophies,
        Sparks,
    }

    public class Item
    {
        public string Id;
        public string ProductId;
        public string Name;
        public string Description;
        public string Category;
        public ItemKind Kind;
        public Currency Currency;
        public int Cost;

        public string ItemIdentifier;
    }
}