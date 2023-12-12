using System.Text;
using System.Globalization;
using AO;

public class GameUI : System<GameUI> 
{
    public bool IsShowingUpgradesWindow;
    public bool IsShowingShopWindow;
    public bool IsShowingPetsWindow;
    public bool IsShowingRebirthWindow;

    public override void Start()
    {
        IsShowingUpgradesWindow = false;
        IsShowingShopWindow = false;
        IsShowingPetsWindow = false;
        IsShowingRebirthWindow = false;
    }

    public PetManager.OwnedPet SelectedPet = null;
    public float SelectedPetDisplayT;

    public void DoSingleStatUI(ref Rect rect, string id, UI.TextSettings textSettings, string str, string tooltipText) {
        var rowRect = rect.CutTop(30);
        rowRect = UI.Text(rowRect, str, textSettings);
        UI.TooltipResult tooltip = UI.Tooltip(rowRect, id, out Rect mouseRect, new UI.TooltipSettings());
        if (tooltip.Hovering) {
            UI.PushLayerRelative(1);
            using var _1 = AllOut.Defer(UI.PopLayer);

            var hoverTextSettings = textSettings;
            hoverTextSettings.horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left;
            hoverTextSettings.verticalAlignment = UI.TextSettings.VerticalAlignment.Top;
            hoverTextSettings.wordWrap = false;
            UI.PushColorMultiplier(new Vector4(tooltip.Hover01, tooltip.Hover01, tooltip.Hover01, tooltip.Hover01));
            using var _2 = AllOut.Defer(UI.PopColorMultiplier);
            UI.Text(mouseRect, tooltipText, hoverTextSettings);
        }
    }

    public override void Update()
    {
        if (Network.IsServer) return;

        var localPlayer = (FatPlayer) Network.LocalPlayer;
        if (localPlayer == null) return;

        Rect topBarRect = UI.SafeRect.TopCenterRect();
        topBarRect = topBarRect.Grow(0, 500, 65, 500)
                               .Offset(0, -5);

        var textSettings = new UI.TextSettings() 
        {
            font = UI.TextSettings.Font.AlphaKind,
            size = 48,
            color = Vector4.White,
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
            wordWrap = false,
            wordWrapOffset = 0,
            outline = true,
            outlineThickness = 2,
            // dropShadow = false,
            // outline= false,
        };

        {
            var topBarGrid = UI.GridLayout.Make(topBarRect, 3, 1, UI.GridLayout.SizeSource.GRID_SIZE, 0);
            
            var trophiesRect = topBarGrid.Next().Inset(0, 10, 0, 10);
            UI.Image(trophiesRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
            var trophiesIconRect = trophiesRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3).Offset(6, 0);
            UI.Image(trophiesIconRect, References.Instance.CoinIcon, Vector4.White, new UI.NineSlice());
            textSettings.color = References.Instance.YellowText;
            UI.Text(trophiesRect, $"{localPlayer.Trophies}", textSettings);
            
            var cashRect = topBarGrid.Next().Inset(0, 10, 0, 10);
            UI.Image(cashRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
            var cashIconRect = cashRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3).Offset(6, 0);
            UI.Image(cashIconRect, References.Instance.CoinIcon, Vector4.White, new UI.NineSlice());
            textSettings.color = References.Instance.GreenText;
            UI.Text(cashRect, $"{localPlayer.Coins}", textSettings);

            var foodRect = topBarGrid.Next().Inset(0, 10, 0, 10);
            UI.Image(foodRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
            var foodIconRect = foodRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3).Offset(6, 0);
            UI.Image(foodIconRect, References.Instance.FoodIcon, Vector4.White, new UI.NineSlice());
            textSettings.color = References.Instance.RedText;
            UI.Text(foodRect, $"{localPlayer.Food}/{localPlayer.ModifiedMaxFood}", textSettings);
        }

        // top-left stats
        {
            var statsTextSettings = textSettings;
            statsTextSettings.horizontalAlignment = UI.TextSettings.HorizontalAlignment.Right;
            statsTextSettings.verticalAlignment = UI.TextSettings.VerticalAlignment.Top;
            statsTextSettings.size = 28;
            statsTextSettings.wordWrap = true;
            var statsRect = UI.SafeRect.TopLeftRect().GrowRight(200);
            StringBuilder sb = new StringBuilder();

            DoSingleStatUI(ref statsRect, "player",  statsTextSettings, $"Player Level: {localPlayer.PlayerLevel}",    $"");
            DoSingleStatUI(ref statsRect, "stomach", statsTextSettings, $"Stomach Level: {localPlayer._maxFoodLevel}", $"Stomach Size: {localPlayer.ModifiedMaxFood}");
            DoSingleStatUI(ref statsRect, "mouth",   statsTextSettings, $"Mouth Level: {localPlayer._mouthSizeLevel}", $"Mouth Size: {localPlayer.ModifiedMouthSize}");
            DoSingleStatUI(ref statsRect, "chew",    statsTextSettings, $"Chew Level: {localPlayer._chewSpeedLevel}",  $"Chew Speed: {localPlayer.ModifiedChewSpeed}x");
            DoSingleStatUI(ref statsRect, "rebirth", statsTextSettings, $"Rebirth Level: {localPlayer._rebirth}",      $"Cash Multiplier: {localPlayer.RebirthCashMultiplier}x");
        }

        var sideBarRect = UI.SafeRect.LeftRect();
        sideBarRect = sideBarRect.GrowRight(200)
                                 .Inset(300, 0, 300, 0);

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
            // dropShadow = false,
            // outline= false,
        };

        UI.PushId("SIDEBAR");
    
        var upgradesButtonRect = sideBarRect.CutTop(100).Inset(10, 10, 10, 10);
        var upgradesButtonResult = UI.Button(upgradesButtonRect, "Upgrades", buttonSettings, buttonTextSettings);
        if (upgradesButtonResult.clicked) 
        {
            IsShowingUpgradesWindow = !IsShowingUpgradesWindow;
        }

        var shopButtonRect = sideBarRect.CutTop(100).Inset(10, 10, 10, 10);
        var shopButtonResult = UI.Button(shopButtonRect, "Shop", buttonSettings, buttonTextSettings);
        if (shopButtonResult.clicked) 
        {
            IsShowingShopWindow = !IsShowingShopWindow;
        }

        var petsButtonRect = sideBarRect.CutTop(100).Inset(10, 10, 10, 10);
        var petsButtonResult = UI.Button(petsButtonRect, "Pets", buttonSettings, buttonTextSettings);
        if (petsButtonResult.clicked) 
        {
            IsShowingPetsWindow = !IsShowingPetsWindow;
        }

        var rebirthButtonRect = sideBarRect.CutTop(100).Inset(10, 10, 10, 10);
        var rebirthButtonResult = UI.Button(rebirthButtonRect, "Rebirth", buttonSettings, buttonTextSettings);
        if (rebirthButtonResult.clicked) 
        {
            IsShowingRebirthWindow = !IsShowingRebirthWindow;
        }

        UI.PopId();

        if (IsShowingUpgradesWindow) 
        {
            UI.PushId("UPGRADES_WINDOW");
            using var _1 = AllOut.Defer(UI.PopId);

            if (UI.Button(UI.ScreenRect, "", new UI.ButtonSettings(), new UI.TextSettings()).clicked) 
            {
                IsShowingUpgradesWindow = false;
            }

            var windowRect = UI.SafeRect.CenterRect();
            windowRect = windowRect.Grow(200, 300, 200, 300);
            UI.Blocker(windowRect, "upgrades");
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});

            if (localPlayer.MaxFoodLevel < FatPlayer.StomachSizeByLevel.Length)
            {
                var upgradeStomachRect = windowRect.CutTopUnscaled(100).Inset(10, 10, 10, 10);
                var nextStomachCost = FatPlayer.StomachSizeByLevel[localPlayer.MaxFoodLevel].Cost;
                var upgradeStomachResult = UI.Button(upgradeStomachRect, $"Stomach Lv. {localPlayer.MaxFoodLevel+1} - Cost: {nextStomachCost}", buttonSettings, buttonTextSettings);
                if (upgradeStomachResult.clicked)
                {
                    localPlayer.CallServer_RequestPurchaseStomachSize();
                }
            }

            if (localPlayer.MouthSizeLevel < FatPlayer.MouthSizeByLevel.Length)
            {
                var upgradeMouthSizeRect = windowRect.CutTopUnscaled(100).Inset(10, 10, 10, 10);
                var nextMouthSizeCost = FatPlayer.MouthSizeByLevel[localPlayer.MouthSizeLevel].Cost;
                var upgradeMouthSizeResult = UI.Button(upgradeMouthSizeRect, $"Mouth Size Lv. {localPlayer.MouthSizeLevel+1} - Cost: {nextMouthSizeCost}", buttonSettings, buttonTextSettings);
                if (upgradeMouthSizeResult.clicked)
                {
                    localPlayer.CallServer_RequestPurchaseMouthSize();
                }
            }

            if (localPlayer.ChewSpeedLevel < FatPlayer.ChewSpeedByLevel.Length)
            {
                var upgradeChewSpeedRect = windowRect.CutTopUnscaled(100).Inset(10, 10, 10, 10);
                var nextChewSpeedCost = FatPlayer.ChewSpeedByLevel[localPlayer.ChewSpeedLevel].Cost;
                var upgradeChewSpeedResult = UI.Button(upgradeChewSpeedRect, $"Chew Speed Lv. {localPlayer.ChewSpeedLevel+1} - Cost: {nextChewSpeedCost}", buttonSettings, buttonTextSettings);
                if (upgradeChewSpeedResult.clicked)
                {
                    localPlayer.CallServer_RequestPurchaseChewSpeed();
                }
            }
        }

        if (IsShowingShopWindow) 
        {
            UI.PushId("SHOP_WINDOW");
            using var _1 = AllOut.Defer(UI.PopId);

            IsShowingShopWindow = Shop.Instance.DrawShop();
        }

        if (IsShowingPetsWindow) 
        {
            UI.PushId("PETS_WINDOW");
            using var _1 = AllOut.Defer(UI.PopId);
            
            if (UI.Button(UI.ScreenRect, "", new UI.ButtonSettings(), new UI.TextSettings()).clicked) 
            {
                IsShowingPetsWindow = false;
            }

            var windowRect = UI.SafeRect.CenterRect();
            windowRect = windowRect.Grow(300, 650, 300, 650);
            UI.Blocker(windowRect, "pets");
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});

            var iconRect = windowRect.TopLeftRect().Grow(40, 40, 40, 40).Offset(0, -5);
            UI.Image(iconRect, References.Instance.PetBrown, Vector4.White, new UI.NineSlice());
            var textRect = iconRect.CenterRect().Grow(25, 0, 25, 0).Offset(25, 0);
            UI.Text(textRect, "Pets", new UI.TextSettings(){
                color = References.Instance.BlueText,
                outline = true,
                outlineThickness = 2,
                horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left,
                verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                size = 60,
            });

            var exitRect = windowRect.TopRightRect().Grow(20, 20, 20, 20).Offset(-35, -35);
            var exitResult = UI.Button(exitRect, "EXIT_BUTTON", new UI.ButtonSettings(){ sprite = References.Instance.X }, new UI.TextSettings(){size = 0, color = Vector4.Zero});
            if (exitResult.clicked) 
            {
                IsShowingPetsWindow = false;
            }

            var equippedCapacityRect = windowRect.TopRightRect().Offset(-250, 0).Grow(20, 115, 20, 115);
            var equippedCapacityRectHeight = equippedCapacityRect.Height;
            UI.Image(equippedCapacityRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
            var equippedCapacityIconRect = equippedCapacityRect.LeftRect().Grow(0, equippedCapacityRectHeight/2, 0, equippedCapacityRectHeight/2).Grow(10, 10, 10, 10);
            UI.Image(equippedCapacityIconRect, References.Instance.Backpack, Vector4.White, new UI.NineSlice());
            var equippedCapacityTextRect = equippedCapacityRect.LeftRect().Offset(85, 0);
            UI.Text(equippedCapacityTextRect, $"1/3", new UI.TextSettings(){
                color = References.Instance.YellowText,
                outline = true,
                outlineThickness = 2,
                horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left,
                verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                size = 36,
            });
            var increaseEquippedCapacityRect = equippedCapacityRect.RightRect().Grow(0, equippedCapacityRectHeight/2, 0, equippedCapacityRectHeight/2);
            increaseEquippedCapacityRect = increaseEquippedCapacityRect.Offset(-equippedCapacityRectHeight/2, 0).Inset(5, 10, 5, 0);
            UI.Button(increaseEquippedCapacityRect, "increase_equipped_capacity", new UI.ButtonSettings(){ sprite = References.Instance.Plus }, new UI.TextSettings(){size = 0, color = Vector4.Zero});

            if (SelectedPet != null)
            {
                var selectedPetRect = windowRect.CutRight(500).Inset(75, 0, 75, 0);
                var dividerRect = selectedPetRect.CutLeft(2);
                UI.Image(dividerRect, null, Vector4.Black * 0.25f, new UI.NineSlice());

                var nameRect = selectedPetRect.CutTop(50);
                UI.Text(nameRect, SelectedPet.Name, new UI.TextSettings(){
                    color = Vector4.White,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 48,
                });

                var infoRect = selectedPetRect.CutTop(225);
                UI.Image(infoRect, null, Vector4.Black * 0.2f, new UI.NineSlice());
                var petIconRect = infoRect.CutLeft(infoRect.Width*0.33f);
                petIconRect = petIconRect.CenterRect().Grow(petIconRect.Width/2, petIconRect.Width/2, petIconRect.Width/2, petIconRect.Width/2).Inset(10, 10, 10, 10).Offset(10, 0);
                UI.Image(petIconRect, References.Instance.PetBrown, Vector4.White, new UI.NineSlice());

                var rarityRect = infoRect.CutTop(50);
                UI.Text(rarityRect, "Common", new UI.TextSettings(){
                    color = References.Instance.BlueText,
                    outline = true,
                    outlineThickenss = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36,
                });
                
                infoRect.CutTop(5);
                foreach(var modifier in SelectedPet.GetDefinition().StatModifiers)
                {
                    var statModRect = infoRect.CutTop(50);
                    Vector4 col = Vector4.White;
                    string text = "";

                    switch(modifier.Kind)
                    {
                        case PetData.StatModifierKind.MaxFoodMultiply:
                        {
                            col = References.Instance.RedText;
                            text = $"{modifier.MultiplyValue:0.#}x Stomach Size";
                            break;
                        }
                        case PetData.StatModifierKind.ChewSpeedMultiply:
                        {
                            col = References.Instance.GreenText;
                            text = $"{modifier.MultiplyValue:0.#}x Chew Speed";
                            break;
                        }
                        case PetData.StatModifierKind.MouthSizeMultiply:
                        {
                            col = References.Instance.BlueText;
                            text = $"{modifier.MultiplyValue:0.#}x Mouth Size";
                            break;
                        }
                        case PetData.StatModifierKind.MaxFoodAdd:
                        {
                            col = References.Instance.RedText;
                            text = $"+{modifier.MultiplyValue:0.#} Stomach Size";
                            break;
                        }
                        case PetData.StatModifierKind.ChewSpeedAdd:
                        {
                            col = References.Instance.GreenText;
                            text = $"+{modifier.MultiplyValue:0.#} Chew Speed";
                            break;
                        }
                        case PetData.StatModifierKind.MouthSizeAdd:
                        {
                            col = References.Instance.BlueText;
                            text = $"+{modifier.MultiplyValue:0.#} Mouth Size";
                            break;
                        }
                    }
                    UI.Text(statModRect, text, new UI.TextSettings() {
                        color = col,
                        outline = true,
                        outlineThickenss = 2,
                        horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                        verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
                        size = 24
                    });
                }
                var buttonTs = new UI.TextSettings() {
                    color = Vector4.White,
                    outline = true,
                    outlineThickenss = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36
                };
                var equipButtonRect = selectedPetRect.CutTop(90).Inset(5, 10, 5, 10);
                if (SelectedPet.Equipped) 
                {
                    var unequipButtonResult = UI.Button(equipButtonRect, "Unequip", new UI.ButtonSettings(){ sprite = References.Instance.BlueButton, slice = References.Instance.FrameSlice }, buttonTs);
                    if (unequipButtonResult.clicked) 
                    {
                        localPlayer.CallServer_RequestUnequipPet(SelectedPet.Id);
                    }
                }
                else 
                {
                    var equipButtonResult = UI.Button(equipButtonRect, "Equip", new UI.ButtonSettings(){ sprite = References.Instance.GreenButton, slice = References.Instance.FrameSlice }, buttonTs);
                    if (equipButtonResult.clicked) 
                    {
                        localPlayer.CallServer_RequestEquipPet(SelectedPet.Id);
                    }
                }

                var deleteButtonRect = selectedPetRect.CutTop(90).Inset(5, 10, 5, 10);
                var deleteButtonResult = UI.Button(deleteButtonRect, "Delete", new UI.ButtonSettings(){ sprite = References.Instance.RedButton, slice = References.Instance.FrameSlice }, buttonTs);
                if (deleteButtonResult.clicked) 
                {
                }
            }

            Rect contentRect = windowRect.Inset(75, 50, 5, 50);

            UI.ScrollView scrollView = UI.PushScrollView("pets_scroll_view", contentRect, UI.ScrollViewFlags.Vertical);
            using var _3 = AllOut.Defer(() => UI.PopScrollView());

            var petsRect = scrollView.contentRect.TopRect();
            var grid = UI.GridLayout.Make(petsRect, 165, 165, UI.GridLayout.SizeSource.ELEMENT_SIZE);

            foreach (var pet in localPlayer.PetManager.OwnedPets)
            {
                UI.PushId(pet.Id);
                using var _2 = AllOut.Defer(UI.PopId);

                var petRect = grid.Next();
                petRect = petRect.Inset(5,5,5,5);
                buttonSettings.sprite = References.Instance.FrameWhite;
                
                var petButtonResult = UI.Button(petRect, pet.Name, buttonSettings, References.Instance.NoTextSettings);
                if (petButtonResult.clicked) {
                    SelectedPet = SelectedPet == pet ? null : pet;
                    SelectedPetDisplayT = 0;
                }

                var petIconRect = petRect.Inset(5);
                UI.Image(petIconRect, References.Instance.PetBrown, Vector4.White, new UI.NineSlice());

                UI.ExpandCurrentScrollView(petRect);
            }

            // Make an extra row for buffer at then end of the grid
            for (var j = 0; j < 6; j++) 
            {
                var petRect = grid.Next();
                UI.ExpandCurrentScrollView(petRect);
            }
        }

        if (IsShowingRebirthWindow)
        {
            UI.PushId("REBIRTH_WINDOW");
            using var _ = AllOut.Defer(UI.PopId);

            if (UI.Button(UI.ScreenRect, "", new UI.ButtonSettings(), new UI.TextSettings()).clicked) 
            {
                IsShowingRebirthWindow = false;
            }

            var headerTextSettings = textSettings;
            headerTextSettings.horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left;
            headerTextSettings.verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom;

            var upgradeItemTextSettings = textSettings;
            upgradeItemTextSettings.size = 36;

            var windowRect = UI.SafeRect;
            windowRect = windowRect.Inset(200, 350, 200, 350);
            UI.Blocker(windowRect, "rebirth");
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

            var halfWidth = windowRect.Width / 2;

            var currentRebirthData = Rebirth.Instance.GetRebirthData(localPlayer.Rebirth);
            var nextRebirthData    = Rebirth.Instance.GetRebirthData(localPlayer.Rebirth + 1);

            // Current
            {
                var beforeRect = windowRect.LeftRect().GrowRightUnscaled(halfWidth).Inset(50, 200, 225, 50);
                UI.Image(beforeRect, References.Instance.FrameDark, Vector4.White, References.Instance.FrameSlice);
                
                beforeRect = beforeRect.SubRect(0, 0, 1, 1, 0, 0, 10, 0);
                var h = beforeRect.Height / 5;

                var textRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                headerTextSettings.color = Vector4.White;
                UI.Text(textRect, "Current:", headerTextSettings);

                var trophiesRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(trophiesRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(trophiesRect, $"{Util.FormatDouble(localPlayer.Trophies)}", upgradeItemTextSettings);

                var cashRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.GreenText;
                UI.Text(cashRect, $"{Util.FormatDouble(localPlayer.Coins)}", upgradeItemTextSettings);

                var cashMultiplierRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashMultiplierRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.BlueText;
                UI.Text(cashMultiplierRect, $"{currentRebirthData.CashMultiplier:0.##}", upgradeItemTextSettings);

                var rankRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(rankRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(rankRect, $"{currentRebirthData.RankName}", upgradeItemTextSettings);
            }
            
            // Next
            {
                var afterRect = windowRect.RightRect().GrowLeftUnscaled(halfWidth).Inset(50, 50, 225, 200);
                UI.Image(afterRect, References.Instance.FrameWhite, References.Instance.BlueBg, References.Instance.FrameSlice);

                afterRect = afterRect.SubRect(0, 0, 1, 1, 0, 0, 10, 0);
                var h = afterRect.Height / 5;

                var textRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                headerTextSettings.color = References.Instance.YellowText;
                UI.Text(textRect, "Next:", headerTextSettings);

                var trophiesRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(trophiesRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(trophiesRect, $"{Util.FormatDouble(localPlayer.Trophies - nextRebirthData.TrophiesCost)}", upgradeItemTextSettings);

                var cashRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.GreenText;
                UI.Text(cashRect, $"0", upgradeItemTextSettings);

                var cashMultiplierRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashMultiplierRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.BlueText;
                UI.Text(cashMultiplierRect, $"{nextRebirthData.CashMultiplier:0.##}", upgradeItemTextSettings);

                var rankRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(rankRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(rankRect, $"{nextRebirthData.RankName}", upgradeItemTextSettings);
            }

            {
                var bottomRect = windowRect.BottomRect()
                                           .GrowTopUnscaled(75)
                                           .Offset(0, 50)
                                           .Inset(0, 50, 0, 50);

                var sliderRect = bottomRect.CutLeftUnscaled((bottomRect.Width / 3) * 2);
                UI.Image(sliderRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

                double has   = localPlayer.Trophies;
                double needs = currentRebirthData.TrophiesCost;

                var rebirthProgress = Math.Clamp(has / needs, 0, 1);
                var rebirthProgressRect = sliderRect.Inset(5,5,5,5).SubRect(0, 0, (float)rebirthProgress, 1, 0, 0, 0, 0);
                UI.Image(rebirthProgressRect, References.Instance.BlueFill, Vector4.White, new UI.NineSlice());
                UI.Text(sliderRect, $"{Util.FormatDouble(localPlayer.Trophies)} / {Util.FormatDouble(nextRebirthData.TrophiesCost)}", upgradeItemTextSettings);

                var confirmRebirthButtonRect = bottomRect.CutLeftUnscaled(bottomRect.Width * 0.6f).Inset(0, 25, 0, 15);
                if (UI.Button(confirmRebirthButtonRect, "Rebirth", buttonSettings, buttonTextSettings).clicked)
                {
                    localPlayer.CallServer_RequestRebirth();
                }

                Log.Info($"Rebirth progress: {rebirthProgress}");

                buttonSettings.sprite = References.Instance.OrangeButton;
                var skipRebirthButtonRect = bottomRect;
                if (UI.Button(skipRebirthButtonRect, "Skip", buttonSettings, buttonTextSettings).clicked)
                {
                    // ?
                }
            }
        }

        if (localPlayer.CurrentBoss != null)
        {
            var chewRect = UI.GetPlayerRect(localPlayer);
            chewRect = chewRect.Grow(100, 50, 0, 50).Offset(0, 10);

            var myRect = chewRect.CutLeftUnscaled(chewRect.Width * 0.5f).Inset(0, 10, 0, 0);
            var bossRect = chewRect.Inset(0, 0, 0, 10);

            double myProgress   = Math.Min(1.0, (double)localPlayer.MyProgress   / (double)localPlayer.CurrentBoss.AmountToWin);
            double bossProgress = Math.Min(1.0, (double)localPlayer.BossProgress / (double)localPlayer.CurrentBoss.AmountToWin);

            var myProgressRect   = myRect.Inset(5,5,5,5).SubRect(0, 0, 1, (float)myProgress, 0, 0, 0, 0);
            var bossProgressRect = bossRect.Inset(5,5,5,5).SubRect(0, 0, 1, (float)bossProgress, 0, 0, 0, 0);

            UI.Image(myRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
            UI.Image(myProgressRect, References.Instance.GreenFill, Vector4.White, new UI.NineSlice());

            UI.Image(bossRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
            UI.Image(bossProgressRect, References.Instance.RedFill, Vector4.White, new UI.NineSlice());

            var myTextRect = myRect.SubRect(0, 0, 1, 0, 0, 0, 0, 0).GrowBottom(50);
            var bossTextRect = bossRect.SubRect(0, 0, 1, 0, 0, 0, 0, 0).GrowBottom(50);

            UI.Text(myTextRect, $"{localPlayer.MyProgress}", textSettings);
            UI.Text(bossTextRect, $"{localPlayer.BossProgress}", textSettings);
        }
    }

    public override void Shutdown()
    {
    }
}
