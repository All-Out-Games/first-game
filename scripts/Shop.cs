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
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
            wordWrap = false,
            wordWrapOffset = 0,
            offset = new Vector2(0, 10),
            // dropShadow = false,
            // outline= false,
        };

        var itemNameTextSettings = new UI.TextSettings()
        {
            font = UI.TextSettings.Font.AlphaKind,
            size = 48,
            color = Vector4.Black,
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
        };

        var windowRect = UI.SafeRect.CenterRect();
        windowRect = windowRect.Grow(200, 300, 200, 300);
        UI.Blocker(windowRect, "shop");
        UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});

        var shopCategories = ShopData.ShopEntries.Select(s => s.Category).Distinct().ToList();

        var topBarRect = windowRect.CutTop(75).Offset(0, 25);
        foreach (var category in shopCategories)
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

        Rect contentRect = windowRect;

        UI.ScrollView scrollView = UI.PushScrollView(SelectedCategory, contentRect, UI.ScrollViewFlags.Vertical); {
            using var _ = AllOut.Defer(() => UI.PopScrollView());
            var itemsRect = scrollView.contentRect.TopRect().Inset(0, 5, 0, 5);
            int itemNumber = 0;
            foreach (var shopEntry in shopItems)
            {
                itemNumber += 1;
                UI.PushId(itemNumber.ToString());
                using var _1 = AllOut.Defer(UI.PopId);

                var itemRect = itemsRect.CutTop(75);
                var item = ShopData.Items.First(i => i.Id == shopEntry.ItemId);
                var itemButtonRect = itemRect.CutRight(200).Inset(5);
                var itemNameRect = itemRect;

                UI.Text(itemNameRect, shopEntry.ItemId, itemNameTextSettings);
                if (UI.Button(itemButtonRect, $"Buy: ${item.Cost}", buttonSettings, buttonTextSettings).clicked)
                {
                    if (item.Currency == ShopData.Currency.Coins || item.Currency == ShopData.Currency.Trophies)
                    {
                        localPlayer.CallServer_RequestPurchaseItem(item.Id);
                    }

                    if (item.Currency == ShopData.Currency.Sparks)
                    {
                        Purchasing.PromptPurchase(item.ProductId);
                    }
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
                if (player.IsLocal)
                {
                    Notifications.Show("You don't have enough coins!");
                }
                return;
            }
        }

        if (item.Currency == ShopData.Currency.Trophies) 
        {
            if (player.Trophies < item.Cost)
            {
                if (player.IsLocal)
                {
                    Notifications.Show("You don't have enough trophies!");
                }
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
    public static List<ShopEntry> ShopEntries = new List<ShopEntry>()
    {
        new () { Id = "egg_1a_item", Category = "Eggs", ItemId = "egg_1a" },
        new () { Id = "egg_1b_item", Category = "Eggs", ItemId = "egg_1b" },
        new () { Id = "egg_1c_item", Category = "Eggs", ItemId = "egg_1c" },
        new () { Id = "egg_1b_item", Category = "Eggs", ItemId = "egg_1b" },

        new () { Id = "egg_2a_item", Category = "Eggs", ItemId = "egg_2a" },
        new () { Id = "egg_2b_item", Category = "Eggs", ItemId = "egg_2b" },
        new () { Id = "egg_2c_item", Category = "Eggs", ItemId = "egg_2c" },
        new () { Id = "egg_2b_item", Category = "Eggs", ItemId = "egg_2b" },

        new () { Id = "egg_3a_item", Category = "Eggs", ItemId = "egg_3a" },
        new () { Id = "egg_3b_item", Category = "Eggs", ItemId = "egg_3b" },
        new () { Id = "egg_3c_item", Category = "Eggs", ItemId = "egg_3c" },
        new () { Id = "egg_3b_item", Category = "Eggs", ItemId = "egg_3b" },
    };

    public static List<Item> Items = new List<Item>()
    {
        new () { Id = "egg_1a", ProductId = "", Name = "Egg 1A", Description = "A fun egg", Currency = Currency.Coins, Cost = 5,   Kind = ItemKind.Egg, ItemIdentifier = "egg1a" },
        new () { Id = "egg_1b", ProductId = "", Name = "Egg 1B", Description = "A fun egg", Currency = Currency.Coins, Cost = 25,  Kind = ItemKind.Egg, ItemIdentifier = "egg1b" },
        new () { Id = "egg_1c", ProductId = "", Name = "Egg 1C", Description = "A fun egg", Currency = Currency.Coins, Cost = 150, Kind = ItemKind.Egg, ItemIdentifier = "egg1c" },
        new () { Id = "egg_1b", ProductId = "", Name = "Egg 1D", Description = "A fun egg", Currency = Currency.Coins, Cost = 800, Kind = ItemKind.Egg, ItemIdentifier = "egg1d" },

        new () { Id = "egg_2a", ProductId = "", Name = "Egg 2A", Description = "A fun egg", Currency = Currency.Coins, Cost = 4500,    Kind = ItemKind.Egg, ItemIdentifier = "egg2a" },
        new () { Id = "egg_2b", ProductId = "", Name = "Egg 2B", Description = "A fun egg", Currency = Currency.Coins, Cost = 45000,   Kind = ItemKind.Egg, ItemIdentifier = "egg2b" },
        new () { Id = "egg_2c", ProductId = "", Name = "Egg 2C", Description = "A fun egg", Currency = Currency.Coins, Cost = 275000,  Kind = ItemKind.Egg, ItemIdentifier = "egg2c" },
        new () { Id = "egg_2b", ProductId = "", Name = "Egg 2D", Description = "A fun egg", Currency = Currency.Coins, Cost = 2800000, Kind = ItemKind.Egg, ItemIdentifier = "egg2d" },

        new () { Id = "egg_3a", ProductId = "", Name = "Egg 3A", Description = "A fun egg", Currency = Currency.Coins, Cost = 5000000,     Kind = ItemKind.Egg, ItemIdentifier = "egg3a" },
        new () { Id = "egg_3b", ProductId = "", Name = "Egg 3B", Description = "A fun egg", Currency = Currency.Coins, Cost = 42000000,    Kind = ItemKind.Egg, ItemIdentifier = "egg3b" },
        new () { Id = "egg_3c", ProductId = "", Name = "Egg 3C", Description = "A fun egg", Currency = Currency.Coins, Cost = 680000000,   Kind = ItemKind.Egg, ItemIdentifier = "egg3c" },
        new () { Id = "egg_3b", ProductId = "", Name = "Egg 3D", Description = "A fun egg", Currency = Currency.Coins, Cost = 15000000000, Kind = ItemKind.Egg, ItemIdentifier = "egg3d" },
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
        public long Cost;

        public string ItemIdentifier;
    }
}