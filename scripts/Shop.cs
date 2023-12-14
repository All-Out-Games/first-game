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

        var buttonSettings = new UI.ButtonSettings() 
        {
            color = Vector4.White,
            clickedColor = Vector4.White * 0.7f,
            hoverColor = Vector4.White * 0.9f,
            pressedColor = Vector4.White * 0.5f,
            sprite = References.Instance.GreenButton,
            slice = new UI.NineSlice() { slice = new Vector4(12, 15, 48, 48), sliceScale = 1f },
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
        UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});

        {
            var scrollRect = windowRect.Inset(5, 20, 5, 20);
            var scrollView = UI.PushScrollView("pets_scroll_view", scrollRect, new UI.ScrollViewSettings() { Vertical = true, ClipPadding = new Vector4(5,5,5,5) });
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

                    if (entry.DisplaySize == ShopData.ItemDisplaySize.SingleBigEntry)
                    {
                        var show = entry.ShowIfOwned == "" || entry.ShowIfOwned == null || Purchasing.OwnsGamePassLocal(entry.ShowIfOwned);
                        if (show)
                        {
                            var entryRect = scrollView.contentRect.CutTop(325);
                            UI.Image(entryRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

                            var titleTextRect = entryRect.TopRect().GrowBottom(75);
                            UI.Text(titleTextRect, product.Name, new UI.TextSettings() { size = 60, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                            var descriptionTextRect = entryRect.BottomRect().GrowTop(75).Offset(0, 10);
                            UI.Text(descriptionTextRect, product.Description, new UI.TextSettings() { size = 48, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                            var iconSize = 125;
                            var widthDiff = entryRect.Width - (entry.Icons.Count * iconSize);
                            var iconsRect = entryRect.SubRect(0, 0.5f, 0, 0.5f, 0, 0, 0, 0).Grow(iconSize/2, 0, iconSize/2, 0).Offset(widthDiff / 2, 0);
                            var iconGrid = UI.GridLayout.Make(iconsRect, iconSize, iconSize, UI.GridLayout.SizeSource.ELEMENT_SIZE);
                            foreach (var icon in entry.Icons)
                            {
                                var iconRect = iconGrid.Next();
                                UI.Image(iconRect, References.Instance.FrameDark, Vector4.White, References.Instance.FrameSlice);
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
                        }
                    }

                    if (entry.DisplaySize == ShopData.ItemDisplaySize.TripleEntry)
                    {
                        var entryWidth = scrollView.contentRect.Width / 3;

                        if (multiEntryColumnIdx == 0)
                        {
                            multiEntryPreviousRect = scrollView.contentRect.CutTop(400).LeftRect().CutLeft(entryWidth);
                        }
                        else
                        {
                            multiEntryPreviousRect = multiEntryPreviousRect.Offset(entryWidth, 0);
                        }

                        var entryRect = multiEntryPreviousRect.Inset(5);

                        var buyResult = UI.Button(entryRect, "BUY_BUTTON", new UI.ButtonSettings(), References.Instance.NoTextSettings);
                        if (buyResult.clicked)
                        {
                            Purchasing.PromptPurchase(item.ProductId);
                        }
                        
                             if (buyResult.pressed)  entryRect = entryRect.Inset(5);
                        else if (buyResult.hovering) entryRect = entryRect.Grow(8);
                        
                        UI.Image(entryRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

                        var titleTextRect = entryRect.CutTop(100);
                        UI.Text(titleTextRect, product.Name, new UI.TextSettings() { size = 72, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                        var descriptionAndIconRect = entryRect.CutTop(150).Offset(0, -25);
                        var iconRect = descriptionAndIconRect.CutLeft(descriptionAndIconRect.Width * 0.4f).FitAspect(1.0f, Rect.FitAspectKind.KEEP_WIDTH).Offset(20, 0);
                        var descriptionRect = descriptionAndIconRect.Inset(0, 10, 0, 10);
                        UI.Image(iconRect, References.Instance.FrameDark, Vector4.White, References.Instance.FrameSlice);
                        UI.Text(descriptionRect, product.Description, new UI.TextSettings() { size = 42, color = Vector4.White, outline = true, outlineThickness = 2, wordWrap = true, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                        var priceRect = entryRect.CutBottom(100).Offset(0, 10);
                        UI.Text(priceRect, $"{product.Price}", new UI.TextSettings() { size = 72, color = Vector4.White, outline = true, outlineThickness = 2, horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center, verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom });

                        if (Purchasing.OwnsGamePassLocal(item.ProductId))
                        {
                            var checkmarkRect = entryRect.TopLeftRect().Grow(0, 50, 50, 0).Offset(10, -10);
                            UI.Image(checkmarkRect, References.Instance.CheckMark, Vector4.White, new UI.NineSlice());
                        }

                        multiEntryColumnIdx += 1;
                        if (multiEntryColumnIdx >= 3)
                        {
                            multiEntryColumnIdx = 0;
                        }
                    }
                }
            }
            UI.ExpandCurrentScrollView(scrollView.contentRect.CutTop(75));
        }

        // Window Title
        {
            var iconRect = windowRect.TopLeftRect().Grow(40, 40, 40, 40).Offset(0, -5);
            UI.Image(iconRect, References.Instance.Shop, Vector4.White, new UI.NineSlice());
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

        // var shopCategories = ShopData.ShopEntries.Select(s => s.Category).Distinct().ToList();

        // var topBarRect = windowRect.CutTop(75).Offset(0, 25);
        // foreach (var category in shopCategories)
        // {
        //     var categoryButtonRect = topBarRect.CutLeft(150).Inset(0, 5, 0, 5);
        //     var categoryButtonResult = UI.Button(categoryButtonRect, category, buttonSettings, buttonTextSettings);
        //     if (categoryButtonResult.clicked) 
        //     {
        //         SelectedCategory = category;
        //     }
        // }

        // FatPlayer localPlayer = (FatPlayer) Network.LocalPlayer;

        // var shopItems = ShopData.ShopEntries.Where(s => s.Category == SelectedCategory).ToList();

        // Rect contentRect = windowRect;

        // UI.ScrollView scrollView = UI.PushScrollView(SelectedCategory, contentRect, UI.ScrollViewFlags.Vertical); {
        //     using var _ = AllOut.Defer(() => UI.PopScrollView());
        //     var itemsRect = scrollView.contentRect.TopRect().Inset(0, 5, 0, 5);
        //     foreach (var shopEntry in shopItems)
        //     {
        //         var itemRect = itemsRect.CutTop(75);
        //         var item = ShopData.Items.First(i => i.Id == shopEntry.ItemId);
        //         var itemButtonRect = itemRect.CutRight(200).Inset(5);
        //         var itemNameRect = itemRect;

        //         UI.Text(itemNameRect, shopEntry.ItemId, itemNameTextSettings);
        //         if (UI.Button(itemButtonRect, $"Buy: ${item.Cost}", buttonSettings, buttonTextSettings).clicked)
        //         {
        //             if (item.Currency == ShopData.Currency.Coins || item.Currency == ShopData.Currency.Trophies)
        //             {
        //                 localPlayer.CallServer_RequestPurchaseItem(item.Id);
        //             }

        //             if (item.Currency == ShopData.Currency.Sparks)
        //             {
        //                 Purchasing.PromptPurchase(item.ProductId);
        //             }
        //         }
        //     }
        // }
        
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
            if (!PetData.Eggs.TryGetValue(item.Id, out var eggDefinition))
            {
                Log.Error($"Could not find egg definition for {item.Id}");
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
                Log.Error($"Could not select pet from egg {item.Id}");
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
                new () { ItemId = "vip", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "vip", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "vip", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },

                new () { ItemId = "vip", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "vip", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "vip", Background = "vip_background", Icons = new() { "" }, DisplaySize = ItemDisplaySize.TripleEntry },
            }
        },

        new ShopCategory()
        {
            Name = "Trophies",
            Entries = new List<ShopEntry>()
            {
                new () { ItemId = "giant_trophy_pack", Background = "trophy_giant", Icons = new () { "" }, DisplaySize = ItemDisplaySize.SingleBigEntry },

                new () { ItemId = "small_trophy",  Background = "trophy_background", Icons = new() { "trophy_small" },  DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "medium_trophy", Background = "trophy_background", Icons = new() { "trophy_medium" }, DisplaySize = ItemDisplaySize.TripleEntry },
                new () { ItemId = "large_trophy",  Background = "trophy_background", Icons = new() { "trophy_large" },  DisplaySize = ItemDisplaySize.TripleEntry },
            }
        }
    };

    public static List<Item> Items = new List<Item>()
    {
        new () { Id = "egg1a", ProductId = "1234", Name = "Egg 1A", Description = "A fun egg", Currency = Currency.Trophies, Cost = 5,   Kind = ItemKind.Egg },
        new () { Id = "egg1b", ProductId = "1234", Name = "Egg 1B", Description = "A fun egg", Currency = Currency.Trophies, Cost = 25,  Kind = ItemKind.Egg },
        new () { Id = "egg1c", ProductId = "1234", Name = "Egg 1C", Description = "A fun egg", Currency = Currency.Trophies, Cost = 150, Kind = ItemKind.Egg },
        new () { Id = "egg1b", ProductId = "1234", Name = "Egg 1D", Description = "A fun egg", Currency = Currency.Trophies, Cost = 800, Kind = ItemKind.Egg },

        new () { Id = "egg2a", ProductId = "1234", Name = "Egg 2A", Description = "A fun egg", Currency = Currency.Trophies, Cost = 4500,    Kind = ItemKind.Egg },
        new () { Id = "egg2b", ProductId = "1234", Name = "Egg 2B", Description = "A fun egg", Currency = Currency.Trophies, Cost = 45000,   Kind = ItemKind.Egg },
        new () { Id = "egg2c", ProductId = "1234", Name = "Egg 2C", Description = "A fun egg", Currency = Currency.Trophies, Cost = 275000,  Kind = ItemKind.Egg },
        new () { Id = "egg2b", ProductId = "1234", Name = "Egg 2D", Description = "A fun egg", Currency = Currency.Trophies, Cost = 2800000, Kind = ItemKind.Egg },

        new () { Id = "egg3a", ProductId = "1234", Name = "Egg 3A", Description = "A fun egg", Currency = Currency.Trophies, Cost = 5000000,     Kind = ItemKind.Egg },
        new () { Id = "egg3b", ProductId = "1234", Name = "Egg 3B", Description = "A fun egg", Currency = Currency.Trophies, Cost = 42000000,    Kind = ItemKind.Egg },
        new () { Id = "egg3c", ProductId = "1234", Name = "Egg 3C", Description = "A fun egg", Currency = Currency.Trophies, Cost = 680000000,   Kind = ItemKind.Egg },
        new () { Id = "egg3b", ProductId = "1234", Name = "Egg 3D", Description = "A fun egg", Currency = Currency.Trophies, Cost = 15000000000, Kind = ItemKind.Egg },
        new () { Id = "egg4a", ProductId = "1234", Name = "Egg 4A", Description = "A fun egg", Currency = Currency.Trophies, Cost = 50000000000,     Kind = ItemKind.Egg },

        new () { Id = "starter_pack1", ProductId = "657a687d43a4e98882a2ee91", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Pass, },
        new () { Id = "starter_pack2", ProductId = "657a688643a4e98882a2ee92", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Pass, },
        new () { Id = "starter_pack3", ProductId = "657a688e43a4e98882a2ee93", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Pass, },

        new () { Id = "vip", ProductId = "657a67e843a4e98882a2ee8e", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Pass, },
        new () { Id = "2x_trophies", ProductId = "657a67ff43a4e98882a2ee90", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Pass, },
        new () { Id = "2x_food", ProductId = "657a67f443a4e98882a2ee8f", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Pass, },

        new () { Id = "small_trophy",      ProductId = "657a679c43a4e98882a2ee88", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Egg, },
        new () { Id = "medium_trophy",     ProductId = "657a67ae43a4e98882a2ee89", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Egg, },
        new () { Id = "large_trophy",      ProductId = "657a67b843a4e98882a2ee8a", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Egg, },
        new () { Id = "giant_trophy_pack", ProductId = "657a67c743a4e98882a2ee8b", Name = "", Description = "", Currency = Currency.Sparks, Cost = 0, Kind = ItemKind.Egg, },

    };

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
    }
}
