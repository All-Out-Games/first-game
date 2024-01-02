using AO;

public class Shop : System<Shop>
{
    public Random Random;

    public string ScrollToIAP;

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

        var buttonSettings = new UI.ButtonSettings() 
        {
            color = Vector4.White,
            clickedColor = Vector4.White * 0.7f,
            hoverColor = Vector4.White * 0.9f,
            pressedColor = Vector4.White * 0.5f,
            sprite = References.Instance.GreenButton,
            slice = References.Instance.ButtonSlice,
        };

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
        windowRect = windowRect.Grow(350, 600, 350, 600);
        UI.Blocker(windowRect, "shop");
        UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

        {
            var scrollRect = windowRect.Inset(5, 20, 5, 20);
            var scrollView = UI.PushScrollView("pets_scroll_view", scrollRect, new UI.ScrollViewSettings() { Vertical = true, ClipPadding = new Vector4(0,5,0,5) });
            using var _ = AllOut.Defer(UI.PopScrollView);

            var idx = 0;
            foreach (var category in ShopData.ShopCategories)
            {
                var categoryTitleRect = scrollView.contentRect.CutTop(75);
                UI.Text(categoryTitleRect, category.Name, new UI.TextSettings() { size = 60, color = References.Instance.BlueText, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                var multiEntryColumnIdx = 0;
                var multiEntryPreviousRect = new Rect();
                foreach(var entry in category.Entries)
                {
                    var item = ShopData.Items.Find(i => i.Id == entry.ItemId);
                    if (item == null)
                    {
                        Log.Error($"Could not find item {entry.ItemId}");
                        continue;
                    }

                    var product = Purchasing.GetProduct(item.ProductId);
                    if (!product.IsValid())
                    {
                        continue;
                    }

                    if (product.IsGamePass && Purchasing.OwnsGamePassLocal(item.ProductId))
                    {
                        continue;
                    }

                    UI.PushId($"{entry.ItemId}_{idx++}");
                    using var __ = AllOut.Defer(UI.PopId);

                    var show = true;
                    if (entry.ShowIfOwned.Has())
                    {
                        show = Purchasing.OwnsGamePassLocal(entry.ShowIfOwned);
                    }
                    if (show)
                    {
                        Rect entryRect = new Rect();
                        bool displaySizeWasValid = true;
                        switch (entry.DisplaySize)
                        {
                            case ShopData.ItemDisplaySize.SingleBigEntry:
                            {
                                entryRect = scrollView.contentRect.CutTop(325);
                                UI.Image(entryRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

                                var titleTextRect = entryRect.TopRect().GrowBottom(75);
                                UI.Text(titleTextRect, product.Name, new UI.TextSettings() { size = 60, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom, wordWrap = true });

                                var descriptionTextRect = entryRect.BottomRect().GrowTop(75).Offset(0, 10);
                                UI.Text(descriptionTextRect, product.Description, new UI.TextSettings() { size = 48, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                                var iconSize = 125;
                                var widthDiff = entryRect.Width - (entry.Icons.Count * iconSize);
                                var iconsRect = entryRect.SubRect(0, 0.5f, 0, 0.5f, 0, 0, 0, 0).Grow(iconSize/2, 0, iconSize/2, 0).Offset(widthDiff / 2, 0);
                                var iconGrid = UI.GridLayout.Make(iconsRect, iconSize, iconSize, UI.GridLayout.SizeSource.ELEMENT_SIZE);

                                var pack = ShopData.Packs.FirstOrDefault(p => p.Id == item.Id);
                                if (pack != null)
                                {
                                    foreach (var packItemId in pack.Items)
                                    {
                                        var packItem = ShopData.Items.FirstOrDefault(i => i.Id == packItemId);
                                        if (packItem == null) continue;

                                        var packProduct = Purchasing.GetProduct(packItem.ProductId);
                                        if (!packProduct.IsValid()) continue;

                                        var iconRect = iconGrid.Next();
                                        UI.Image(iconRect, packProduct.Icon, Vector4.White, References.Instance.FrameSlice);
                                    }
                                }
                                else
                                {
                                    foreach (var icon in entry.Icons)
                                    {
                                        var iconRect = iconGrid.Next();
                                        UI.Image(iconRect, References.Instance.FrameDark, Vector4.White, References.Instance.FrameSlice);
                                    }
                                }

                                var buttonRect = entryRect.BottomRightRect().Grow(100, 0, 0, 200).Offset(-15, 15);
                                var buyButtonTextSettings = new UI.TextSettings()
                                {
                                    size = 48,
                                    color = Vector4.White,
                                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                                    outline = true,
                                    outlineThickness = 2,
                                };
                                if (UI.Button(buttonRect, $"{product.Price}", buttonSettings, buyButtonTextSettings).clicked)
                                {
                                    Purchasing.PromptPurchase(item.ProductId);
                                }
                                multiEntryColumnIdx = 0;
                                break;
                            }
                            case ShopData.ItemDisplaySize.TripleEntry:
                            {
                                var entryWidth = scrollView.contentRect.Width / 3;

                                if (multiEntryColumnIdx == 0)
                                {
                                    multiEntryPreviousRect = scrollView.contentRect.CutTop(400).LeftRect().CutLeftUnscaled(entryWidth);
                                }
                                else
                                {
                                    multiEntryPreviousRect = multiEntryPreviousRect.Slide(1, 0);
                                }

                                entryRect = multiEntryPreviousRect.Inset(5);

                                var buyResult = UI.Button(entryRect, "BUY_BUTTON", new UI.ButtonSettings(), References.Instance.NoTextSettings);
                                if (buyResult.clicked)
                                {
                                    Purchasing.PromptPurchase(item.ProductId);
                                }

                                var entryCutRect = entryRect;
                                     if (buyResult.pressed)  entryRect = entryRect.Inset(3);
                                else if (buyResult.hovering) entryRect = entryRect.Grow(3);

                                UI.Image(entryRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

                                var titleTextRect = entryCutRect.CutTop(100);
                                UI.Text(titleTextRect, product.Name, new UI.TextSettings() { size = 72, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom, wordWrap = true });

                                var descriptionAndIconRect = entryCutRect.CutTop(150).Offset(0, -25);
                                var iconRect = descriptionAndIconRect.CutLeft(descriptionAndIconRect.Width * 0.4f).FitAspect(1.0f, Rect.FitAspectKind.KEEP_WIDTH).Offset(20, 0);
                                var descriptionRect = descriptionAndIconRect.Inset(0, 10, 0, 10);
                                UI.Image(iconRect, product.Icon, Vector4.White);
                                UI.Text(descriptionRect, product.Description, new UI.TextSettings() { size = 42, color = Vector4.White, outline = true, outlineThickness = 2, wordWrap = true, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                                var priceRect = entryCutRect.CutBottom(100).Offset(0, 10);
                                UI.Text(priceRect, $"{product.Price}", new UI.TextSettings() { size = 72, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                                if (Purchasing.OwnsGamePassLocal(item.ProductId))
                                {
                                    var checkmarkRect = entryCutRect.TopLeftRect().Grow(0, 50, 50, 0).Offset(10, -10);
                                    UI.Image(checkmarkRect, References.Instance.CheckMark, Vector4.White);
                                }

                                multiEntryColumnIdx += 1;
                                if (multiEntryColumnIdx >= 3)
                                {
                                    multiEntryColumnIdx = 0;
                                }
                                break;
                            }
                            default: {
                                displaySizeWasValid = false;
                                Log.Error($"Unknown IAP display size: {entry.DisplaySize}");
                                break;
                            }
                        }

                        if (ScrollToIAP.Has() && ScrollToIAP == entry.ItemId)
                        {
                            ScrollToIAP = string.Empty;
                            if (displaySizeWasValid)
                            {
                                scrollView.ScrollTo(entryRect);
                            }
                        }
                    }
                }
            }
            UI.ExpandCurrentScrollView(scrollView.contentRect.CutTop(75));
        }

        ScrollToIAP = string.Empty;

        // Window Title
        {
            var iconRect = windowRect.TopLeftRect().Grow(40, 40, 40, 40).Offset(0, -5);
            UI.Image(iconRect, References.Instance.Shop, Vector4.White);
            var textRect = iconRect.CenterRect().Grow(25, 0, 25, 0).Offset(25, 0);
            UI.Text(textRect, "Shop", new UI.TextSettings(){
                color = References.Instance.BlueText,
                outline = true,
                outlineThickness = 2,
                horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left,
                verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                size = 60,
            });
        }

        var exitRect = windowRect.TopRightRect().Grow(20, 20, 20, 20).Offset(-35, -35);
        var exitResult = UI.Button(exitRect, "EXIT_BUTTON", new UI.ButtonSettings(){ sprite = References.Instance.X }, new UI.TextSettings(){size = 0, color = Vector4.Zero});
        if (exitResult.clicked) 
        {
            return false;
        }
        return true;
    }

    public void Purchase(FatPlayer player, string itemId)
    {
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

        if (Network.IsClient)
        {
            return;
        }

        if (item.Currency == ShopData.Currency.Sparks)
        {
            Log.Error("Tried to purchase sparks item. This must be done through Purchasing.PromptPurchase");
            return;
        }

        // We get here if the item was purchased with coins or trophies and they could afford it

        var (success, _) = GrantItem(player, item);
        if (!success)
        {
            Log.Error($"Failed to grant item: {item.Id}. Cannot complete purhcase!");
            return;
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
        var item = ShopData.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) 
        {
            Log.Error($"Failed to find item: {productId}. Cannot complete purhcase!");
            return false;
        }

        var (success, _) = GrantItem(purchaser, item);
        return success;
    }

    public long THOUSAND(long n)
    {
        return n * 1000;
    }

    public long MILLION(long n)
    {
        return n * 1000 * 1000;
    }

    public (bool, string) GrantItem(Player p, ShopData.Item item, bool allowOpeningEggs = true)
    {
        FatPlayer player = (FatPlayer)p;
        
        if (item.Kind == ShopData.ItemKind.Pass)
        {
            switch (item.Id)
            {
                case "vip":
                case "2x_trophies":
                case "2x_money":
                case "teleporter":
                case "boss_autoclicker":
                case "pet_equip_cap_1":
                case "pet_equip_cap_2":
                case "pet_equip_cap_3":
                case "pet_storage_cap_1":
                case "pet_storage_cap_2":
                case "pet_storage_cap_3":
                case "pet_storage_cap_4":
                case "pet_storage_cap_5":
                {
                    player.ServerOnBuyGamePass(item.Id);
                    return (true, "");
                }
                default:
                    Log.Error($"Unknown pass: {item.Id}");
                    return (false, "");
            }
        }

        if (item.Kind == ShopData.ItemKind.Pack)
        {
            var pack = ShopData.Packs.FirstOrDefault(p => p.Id == item.Id);
            if (pack == null)
            {
                Log.Error($"Could not find pack definition for {item.Id}");
                return (false, "");
            }

            var eggIds = new List<string>();
            var petIds = new List<string>();

            foreach(var itemId in pack.Items)
            {
                var packItem = ShopData.Items.FirstOrDefault(i => i.Id == itemId);
                if (packItem == null)
                {
                    Log.Error($"Could not find pack item definition for {itemId}");
                    continue;
                }

                var (success, data) = GrantItem(player, packItem, false);
                if (!success)
                {
                    Log.Error($"Failed to grant pack item: {packItem.Id}");
                    continue;
                }

                if (packItem.Kind == ShopData.ItemKind.Egg)
                {
                    eggIds.Add(packItem.Id);
                    petIds.Add(data);
                }
            }

            if (eggIds.Count > 0)
            {
                player.CallClient_OpenMultipleEggs(eggIds.ToArray(), petIds.ToArray());
            }
        }

        if (item.Kind == ShopData.ItemKind.Trophies)
        {
            switch (item.Id)
            {
                case "small_trophy": {
                         if (player.Rebirth < 10)  player.Trophies += 5;
                    else if (player.Rebirth < 20)  player.Trophies += THOUSAND(11);
                    else                           player.Trophies += MILLION(21);
                    return (true, "");
                }
                case "medium_trophy": {
                         if (player.Rebirth < 10)  player.Trophies += 10;
                    else if (player.Rebirth < 20)  player.Trophies += THOUSAND(23);
                    else                           player.Trophies += MILLION(43);
                    return (true, "");
                }
                case "large_trophy": {
                         if (player.Rebirth < 10)  player.Trophies += 21;
                    else if (player.Rebirth < 20)  player.Trophies += THOUSAND(26);
                    else                           player.Trophies += MILLION(85);
                    return (true, "");
                }
                case "huge_trophy_pack": {
                         if (player.Rebirth < 10)  player.Trophies += 42;
                    else if (player.Rebirth < 20)  player.Trophies += THOUSAND(92);
                    else                           player.Trophies += MILLION(190);
                    return (true, "");
                }
                case "gigantic_trophy_pack": {
                         if (player.Rebirth < 10)  player.Trophies += 85;
                    else if (player.Rebirth < 20)  player.Trophies += THOUSAND(185);
                    else                           player.Trophies += MILLION(390);
                    return (true, "");
                }
            }
        }

        if (item.Kind == ShopData.ItemKind.Coins)
        {
            switch (item.Id)
            {
                case "small_coin_pack": {
                         if (player.PlayerLevel <= 57)      player.Coins += 5;
                    else if (player.PlayerLevel <= 147)     player.Coins += THOUSAND(1);
                    else                                    player.Coins += MILLION(20);
                    return (true, "");
                }
                case "medium_coin_pack": {
                         if (player.PlayerLevel <= 57)      player.Coins += 11;
                    else if (player.PlayerLevel <= 147)     player.Coins += THOUSAND(2);
                    else                                    player.Coins += MILLION(50);
                    return (true, "");
                }
                case "large_coin_pack": {
                         if (player.PlayerLevel <= 57)      player.Coins += 22;
                    else if (player.PlayerLevel <= 147)     player.Coins += THOUSAND(5);
                    else                                    player.Coins += MILLION(110);
                    return (true, "");
                }
                case "huge_coin_pack": {
                         if (player.PlayerLevel <= 57)      player.Coins += 46;
                    else if (player.PlayerLevel <= 147)     player.Coins += THOUSAND(12);
                    else                                    player.Coins += MILLION(230);
                    return (true, "");
                }
                case "gigantic_coin_pack": {
                         if (player.PlayerLevel <= 57)      player.Coins += 95;
                    else if (player.PlayerLevel <= 147)     player.Coins += THOUSAND(25);
                    else                                    player.Coins += MILLION(500);
                    return (true, "");
                }
            }
        }

        if (item.Kind == ShopData.ItemKind.Egg) 
        {
            if (!PetData.Eggs.TryGetValue(item.Id, out var eggDefinition))
            {
                Log.Error($"Could not find egg definition for {item.Id}");
                return (false, "");
            }

            var totalWeight = eggDefinition.PossiblePets.Sum(p => p.Weight);
            var rnd = Random.Next(0, totalWeight);

            PetData.WeightedPet selectedPet = null;
            int cursor = 0;
            foreach(var pet in eggDefinition.PossiblePets) 
            {
                if (rnd >= cursor && rnd < (cursor + pet.Weight))
                {
                    selectedPet = pet;
                    break;
                }
                cursor += pet.Weight;
            }

            if (selectedPet == null) 
            {
                Log.Error($"Could not select pet from egg {item.Id}");
                return (false, "");
            }

            Guid guid = Guid.NewGuid();
            player.CallClient_AddPet(guid.ToString(), selectedPet.Id, item.Id);

            if (allowOpeningEggs) 
            {
                player.CallClient_OpenEgg(item.Id, selectedPet.Id);
            }
            return (true, selectedPet.Id);
        }

        Log.Error($"Unhandled item kind: {item.Kind}. Id: {item.Id}");
        return (false, "");
    }
}

public static class ShopData
{
    public static List<ShopCategory> ShopCategories = new List<ShopCategory>()
    {
        new ShopCategory() 
        {
            Name = "Packs",
            Entries = new List<ShopEntry>()
            {
                new () { ItemId = "starter_pack1", ShowIfOwned = "",              Background = "pack_starter", Icons = new () { "pack_starter_icon" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
                new () { ItemId = "starter_pack2", ShowIfOwned = "starter_pack1", Background = "pack_starter", Icons = new () { "pack_starter_icon" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
                new () { ItemId = "starter_pack3", ShowIfOwned = "starter_pack2", Background = "pack_starter", Icons = new () { "pack_starter_icon" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
            }
        },

        new ShopCategory()
        {
            Name = "Passes",
            Entries = new List<ShopEntry>()
            {
                new () { ItemId = "vip",               Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "2x_trophies",       Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "2x_money",          Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },

                new () { ItemId = "teleporter",        Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
                new () { ItemId = "boss_autoclicker",  Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },

                new () { ItemId = "pet_equip_cap_1",   Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "pet_equip_cap_2",   Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "pet_equip_cap_3",   Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },

                new () { ItemId = "pet_storage_cap_1", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "pet_storage_cap_2", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "pet_storage_cap_3", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "pet_storage_cap_4", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
                new () { ItemId = "pet_storage_cap_5", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
            }
        },

        new ShopCategory()
        {
            Name = "Trophies",
            Entries = new List<ShopEntry>()
            {
                new () { ItemId = "gigantic_trophy_pack", Background = "trophy_giant", Icons = new () { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
                new () { ItemId = "huge_trophy_pack",     Background = "trophy_giant", Icons = new () { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },

                new () { ItemId = "small_trophy",  Background = "trophy_background", Icons = new() { "trophy_small" },  DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "medium_trophy", Background = "trophy_background", Icons = new() { "trophy_medium" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "large_trophy",  Background = "trophy_background", Icons = new() { "trophy_large" },  DisplaySize = ItemDisplaySize.TripleEntry },
            }
        },

        new ShopCategory()
        {
            Name = "Coins",
            Entries = new List<ShopEntry>()
            {
                new () { ItemId = "gigantic_coin_pack", Background = "coin_giant", Icons = new () { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },
                new () { ItemId = "huge_coin_pack",     Background = "coin_giant", Icons = new () { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },

                new () { ItemId = "small_coin_pack",  Background = "coin_background", Icons = new() { "coin_small" },  DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "medium_coin_pack", Background = "coin_background", Icons = new() { "coin_medium" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "large_coin_pack",  Background = "coin_background", Icons = new() { "coin_large" },  DisplaySize = ItemDisplaySize.TripleEntry },
            }
        },
    };

    public static List<Pack> Packs = new List<Pack>()
    {
        new () { Id = "starter_pack1", Items = new List<string>() { "Fruit Egg" } },
        new () { Id = "starter_pack2", Items = new List<string>() { "Fruit Egg", "Salad Egg" } },
        new () { Id = "starter_pack3", Items = new List<string>() { "Fruit Egg", "Salad Egg", "Sundae Egg", "Mac and Cheese Egg" } },
    };

    public static List<Item> Items = new List<Item>()
    {
        new () { Id = "Fruit Egg",               Name = "Fruit Egg",               Description = "A fun egg", Currency = Currency.Trophies, Cost = 5,           Kind = ItemKind.Egg },
        new () { Id = "Salad Egg",               Name = "Salad Egg",               Description = "A fun egg", Currency = Currency.Trophies, Cost = 25,          Kind = ItemKind.Egg },
        new () { Id = "Sundae Egg",              Name = "Sundae Egg",              Description = "A fun egg", Currency = Currency.Trophies, Cost = 150,         Kind = ItemKind.Egg },
        new () { Id = "Mac and Cheese Egg",      Name = "Mac and Cheese Egg",      Description = "A fun egg", Currency = Currency.Trophies, Cost = 800,         Kind = ItemKind.Egg },

        new () { Id = "Sushi Egg",               Name = "Sushi Egg",               Description = "A fun egg", Currency = Currency.Trophies, Cost = 4500,        Kind = ItemKind.Egg },
        new () { Id = "Pepperoni Pizza Egg",     Name = "Pepperoni Pizza Egg",     Description = "A fun egg", Currency = Currency.Trophies, Cost = 45000,       Kind = ItemKind.Egg },
        new () { Id = "Lasagna Egg",             Name = "Lasagna Egg",             Description = "A fun egg", Currency = Currency.Trophies, Cost = 275000,      Kind = ItemKind.Egg },
        new () { Id = "Burger Egg",              Name = "Burger Egg",              Description = "A fun egg", Currency = Currency.Trophies, Cost = 2800000,     Kind = ItemKind.Egg },

        new () { Id = "Mint Chocolate Chip Egg", Name = "Mint Chocolate Chip Egg", Description = "A fun egg", Currency = Currency.Trophies, Cost = 5000000,     Kind = ItemKind.Egg },
        new () { Id = "Dripping Honeycomb Egg",  Name = "Dripping Honeycomb Egg",  Description = "A fun egg", Currency = Currency.Trophies, Cost = 42000000,    Kind = ItemKind.Egg },
        new () { Id = "Chocolate Cookies Egg",   Name = "Chocolate Cookies Egg",   Description = "A fun egg", Currency = Currency.Trophies, Cost = 680000000,   Kind = ItemKind.Egg },
        new () { Id = "Rainbow Mega Swirl Egg",  Name = "Rainbow Mega Swirl Egg",  Description = "A fun egg", Currency = Currency.Trophies, Cost = 15000000000, Kind = ItemKind.Egg },

        new () { Id = "starter_pack1",        ProductId = "6580a7d41d3ed3881220a6b0", Currency = Currency.Sparks, Kind = ItemKind.Pack, },
        new () { Id = "starter_pack2",        ProductId = "6580a7e01b8adea4b7fe6bea", Currency = Currency.Sparks, Kind = ItemKind.Pack, },
        new () { Id = "starter_pack3",        ProductId = "6580a7ea1b8adea4b7fe6beb", Currency = Currency.Sparks, Kind = ItemKind.Pack, },

        new () { Id = "vip",                  ProductId = "6580ad4e1d3ed3881220a6b1", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "2x_trophies",          ProductId = "6580ad5d1efc21b35e8169d0", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "2x_money",             ProductId = "6581fbd81abd39db1febe7ec", Currency = Currency.Sparks, Kind = ItemKind.Pass, },

        new () { Id = "teleporter",           ProductId = "6580ade01d3ed3881220a6b5", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "boss_autoclicker",     ProductId = "6581fbb69b6ec694b390e7fa", Currency = Currency.Sparks, Kind = ItemKind.Pass, },

        new () { Id = "pet_equip_cap_1",      ProductId = "6580ae111d3ed3881220a6b6", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "pet_equip_cap_2",      ProductId = "6580ae1b91be5316340384f0", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "pet_equip_cap_3",      ProductId = "6580ae24da4e2d44cf18cb4e", Currency = Currency.Sparks, Kind = ItemKind.Pass, },

        new () { Id = "pet_storage_cap_1",    ProductId = "6580ae2e1b8adea4b7fe6bed", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "pet_storage_cap_2",    ProductId = "6580ae377ddd3448b68bf876", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "pet_storage_cap_3",    ProductId = "6580ae4091be5316340384f1", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "pet_storage_cap_4",    ProductId = "6581fa26d9a1a6f408b4f2c9", Currency = Currency.Sparks, Kind = ItemKind.Pass, },
        new () { Id = "pet_storage_cap_5",    ProductId = "6581fa319b6ec694b390e7f8", Currency = Currency.Sparks, Kind = ItemKind.Pass, },

        new () { Id = "skip_rebirth",         ProductId = "6580ae5091be5316340384f2", Currency = Currency.Sparks, Kind = ItemKind.SkipRebirth, },

        new () { Id = "small_coin_pack",      ProductId = "6580ad9b1d3ed3881220a6b3", Currency = Currency.Sparks, Kind = ItemKind.Coins, },
        new () { Id = "medium_coin_pack",     ProductId = "6580adac1b8adea4b7fe6bec", Currency = Currency.Sparks, Kind = ItemKind.Coins, },
        new () { Id = "large_coin_pack",      ProductId = "6580adb91d3ed3881220a6b4", Currency = Currency.Sparks, Kind = ItemKind.Coins, },
        new () { Id = "huge_coin_pack",       ProductId = "6581fb29d9a1a6f408b4f2ca", Currency = Currency.Sparks, Kind = ItemKind.Coins, },
        new () { Id = "gigantic_coin_pack",   ProductId = "6581fb331abd39db1febe7eb", Currency = Currency.Sparks, Kind = ItemKind.Coins, },

        new () { Id = "small_trophy",         ProductId = "6580af2c1b8adea4b7fe6bee", Currency = Currency.Sparks, Kind = ItemKind.Trophies, },
        new () { Id = "medium_trophy",        ProductId = "6580af351d3ed3881220a6b7", Currency = Currency.Sparks, Kind = ItemKind.Trophies, },
        new () { Id = "large_trophy",         ProductId = "6580af3f7ddd3448b68bf877", Currency = Currency.Sparks, Kind = ItemKind.Trophies, },
        new () { Id = "huge_trophy_pack",     ProductId = "6580af5491be5316340384f3", Currency = Currency.Sparks, Kind = ItemKind.Trophies, },
        new () { Id = "gigantic_trophy_pack", ProductId = "6581fb529b6ec694b390e7f9", Currency = Currency.Sparks, Kind = ItemKind.Trophies, },
    };

    public class Pack
    {
        public string Id;
        public List<string> Items;
    }

    public class ShopCategory
    {
        public string Name;
        public List<ShopEntry> Entries;
    }

    public enum ItemDisplaySize
    {
        SingleBigEntry,
        TripleEntry,
    }

    public class ShopEntry
    {
        public string ItemId;
        public string Background;
        public List<string> Icons;
        public bool New;
        public bool Limitedtime;
        public string ShowIfOwned;
        public ItemDisplaySize DisplaySize;
    }

    public enum ItemKind
    {
        Egg,
        Pet,
        Boost,
        Pass,
        Coins,
        Trophies,
        Pack,
        SkipRebirth,
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

        public int IntData;
        public string StringData;
    }
}
