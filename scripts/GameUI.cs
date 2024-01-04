using System.Text;
using System.Globalization;
using AO;

public class GameUI : System<GameUI> 
{
    public const int EggUILayer = 1000;

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
    public PetManager.OwnedPet PetToDelete = null;
    public PetManager.OwnedPet HoveredPet = null;

    public class EggOpeningAnimation
    {
        public string EggId;
        public string ResultPetId;

        public float Timer;
        public Rect MyRect;
        public bool IsNew;
    }

    public List<EggOpeningAnimation> EggOpeningAnimations = new List<EggOpeningAnimation>();

    public static Vector4 RarityColorCommon    = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
    public static Vector4 RarityColorUncommon  = new Vector4(0.5f, 0.8f, 0.5f, 1.0f);
    public static Vector4 RarityColorRare      = new Vector4(0.5f, 0.5f, 0.8f, 1.0f);
    public static Vector4 RarityColorEpic      = new Vector4(0.8f, 0.5f, 0.8f, 1.0f);
    public static Vector4 RarityColorLegendary = new Vector4(0.8f, 0.8f, 0.5f, 1.0f);

    public void DoSingleStatUI(ref Rect rect, string id, UI.TextSettings textSettings, string str, string tooltipText = null, string text2 = null, string text3 = null, string text4 = null)
    {
        var rowRect = rect.CutTop(30);
        rowRect = UI.Text(rowRect, str, textSettings);
        if (!string.IsNullOrEmpty(tooltipText))
        {
            UI.TooltipResult tooltip = UI.Tooltip(rowRect, id, out Rect mouseRect, new UI.TooltipSettings());
            if (tooltip.Hovering) {
                UI.PushColorMultiplier(new Vector4(tooltip.Hover01, tooltip.Hover01, tooltip.Hover01, tooltip.Hover01));
                using var _2 = AllOut.Defer(UI.PopColorMultiplier);

                mouseRect = mouseRect.Offset(30, 0);
                var bgRect = mouseRect;
                {
                    UI.PushLayerRelative(2);
                    using var _1 = AllOut.Defer(UI.PopLayer);

                    var hoverTextSettings = textSettings;
                    hoverTextSettings.horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left;
                    hoverTextSettings.verticalAlignment = UI.TextSettings.VerticalAlignment.Top;
                    hoverTextSettings.wordWrap = false;
                    var textRect = UI.Text(mouseRect, tooltipText, hoverTextSettings);
                    bgRect = bgRect.Encapsulate(textRect);
                    if (!string.IsNullOrEmpty(text2))
                    {
                        mouseRect.CutTop(30);
                        textRect = UI.Text(mouseRect, text2, hoverTextSettings);
                        bgRect = bgRect.Encapsulate(textRect);
                    }
                    if (!string.IsNullOrEmpty(text3))
                    {
                        mouseRect.CutTop(30);
                        textRect = UI.Text(mouseRect, text3, hoverTextSettings);
                        bgRect = bgRect.Encapsulate(textRect);
                    }
                    if (!string.IsNullOrEmpty(text4))
                    {
                        mouseRect.CutTop(30);
                        textRect = UI.Text(mouseRect, text4, hoverTextSettings);
                        bgRect = bgRect.Encapsulate(textRect);
                    }
                }
                {
                    UI.PushLayerRelative(1);
                    using var _1 = AllOut.Defer(UI.PopLayer);
                    UI.Image(bgRect.Grow(5, 5, 5, 5), null, new Vector4(0, 0, 0, 0.8f));
                }
            }
        }
    }

    public static (string, Vector4) BuildStatModifierText(StatModifier modifier)
    {
        Vector4 col = Vector4.White;
        string text = "";

        switch(modifier.Kind)
        {
            case StatModifierKind.StomachSize:
            {
                col = References.Instance.RedText;
                text = $"{modifier.MultiplyValue:0.#}x Stomach Size";
                break;
            }
            case StatModifierKind.ClickPower:
            {
                col = References.Instance.GreenText;
                text = $"{modifier.MultiplyValue:0.#}x Click Power";
                break;
            }
            case StatModifierKind.MouthSize:
            {
                col = References.Instance.BlueText;
                text = $"{modifier.MultiplyValue:0.#}x Mouth Size";
                break;
            }
        }

        return (text, col);
    }

    public void AddEggToOpen(string eggId, string resultPetId)
    {
        EggOpeningAnimations.Add(new EggOpeningAnimation() { EggId = eggId, ResultPetId = resultPetId });
    }

    public static Vector4 GetRarityColor(PetData.Rarity rarity)
    {
        var rarityColor = rarity switch
        {
            PetData.Rarity.Uncommon => RarityColorUncommon,
            PetData.Rarity.Rare => RarityColorRare,
            PetData.Rarity.Epic => RarityColorEpic,
            PetData.Rarity.Legendary => RarityColorLegendary,
            _ => RarityColorCommon,
        };
        return rarityColor;
    }

    public override void Update()
    {
        if (Network.IsServer) return;

        var localPlayer = (FatPlayer) Network.LocalPlayer;
        if (localPlayer == null) return;

        // bottom quest UI
        if (localPlayer.CurrentQuest != null)
        {
            var rect = UI.SafeRect.BottomCenterRect().Grow(0, 200, 0, 200);
            int timeLeft = (int)Math.Ceiling(localPlayer.CurrentQuest.TimeLeft);
            string questText = $"Active Quest: {localPlayer.CurrentQuest.QuestName}. {timeLeft} seconds left!\n{localPlayer.CurrentQuest.Objective} ({localPlayer.CurrentQuest.Progress}/{localPlayer.CurrentQuest.ProgressRequired})\nReward: {localPlayer.CurrentQuest.RewardDescription}";
            UI.Text(rect, questText, UI.TextSettings.Default);
        }

        Rect topBarRect = UI.SafeRect.TopCenterRect();
        topBarRect = topBarRect.Grow(0, 500, 65, 500).Offset(0, -5);

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
            UI.Image(trophiesRect, References.Instance.TopBarBg, Vector4.White, References.Instance.TopBarSlice);
            var trophiesIconRect = trophiesRect.Copy().CutLeft(50).FitAspect(References.Instance.TrophyIcon.Aspect).Inset(3, 3, 3, 3).Offset(9, 0);
            UI.Image(trophiesIconRect, References.Instance.TrophyIcon, Vector4.White);
            textSettings.color = References.Instance.YellowText;
            var trophyTextSettings = textSettings;
            var trophyEase = Ease.OutQuart(Ease.T(Time.TimeSinceStartup - localPlayer.LastTrophyParticleArriveTime, 0.25f));
            trophyTextSettings.size = AOMath.Lerp(trophyTextSettings.size * 1.5f, trophyTextSettings.size, trophyEase);
            UI.Text(trophiesRect, $"{Util.FormatDouble(localPlayer.TrophiesVisual)}", trophyTextSettings);
            
            var cashRect = topBarGrid.Next().Inset(0, 10, 0, 10);
            UI.Image(cashRect, References.Instance.TopBarBg, Vector4.White, References.Instance.TopBarSlice);
            var cashIconRect = cashRect.Copy().CutLeft(50).FitAspect(References.Instance.CoinIcon.Aspect).Inset(3, 3, 3, 3).Offset(9, 0);
            UI.Image(cashIconRect, References.Instance.CoinIcon, Vector4.White);
            textSettings.color = References.Instance.GreenText;
            var coinTextSettings = textSettings;
            var coinEase = Ease.OutQuart(Ease.T(Time.TimeSinceStartup - localPlayer.LastCoinParticleArriveTime, 0.25f));
            coinTextSettings.size = AOMath.Lerp(coinTextSettings.size * 1.5f, coinTextSettings.size, coinEase);
            UI.Text(cashRect, $"{Util.FormatDouble(localPlayer.CoinsVisual)}", coinTextSettings);

            var foodTextSettings = textSettings;
            var foodRect = topBarGrid.Next().Inset(0, 10, 0, 10);
            UI.Image(foodRect, References.Instance.TopBarBg, Vector4.White, References.Instance.TopBarSlice);
            var foodIconRect = foodRect.Copy().CutLeft(50).FitAspect(References.Instance.FoodIcon.Aspect).Inset(3, 3, 3, 3).Offset(9, 0);
            UI.Image(foodIconRect, References.Instance.FoodIcon, Vector4.White);
            foodTextSettings.color = References.Instance.RedText;
            var foodEase = Ease.OutQuart(Ease.T(Time.TimeSinceStartup - localPlayer.LastFoodParticleArriveTime, 0.25f));
            foodTextSettings.size = AOMath.Lerp(foodTextSettings.size * 1.5f, foodTextSettings.size, foodEase);
            UI.Text(foodRect, $"{Util.FormatDouble(localPlayer.AmountOfFoodInStomachVisual)}/{Util.FormatDouble(localPlayer.ModifiedStomachSize)}", foodTextSettings);
        }

        // top-left stats
        {
            var statsTextSettings = textSettings;
            statsTextSettings.horizontalAlignment = UI.TextSettings.HorizontalAlignment.Right;
            statsTextSettings.verticalAlignment = UI.TextSettings.VerticalAlignment.Top;
            statsTextSettings.size = 28;
            statsTextSettings.wordWrap = true;
            var statsRect = UI.SafeRect.TopLeftRect().GrowRight(250);
            StringBuilder sb = new StringBuilder();

            DoSingleStatUI(ref statsRect, "player",  statsTextSettings, $"Player Level: {localPlayer.PlayerLevel}");
            DoSingleStatUI(ref statsRect, "mouth",   statsTextSettings, $"Mouth Level: {localPlayer.MouthSizeLevel}",        $"Mouth Size: {localPlayer.ModifiedMouthSize.ToString("F2")}",            $"Base: {localPlayer.BaseMouthSizeValue.ToString("F2")}",      $"From Pets: {localPlayer.CalculateTotalMultiplierFromPets(StatModifierKind.MouthSize).ToString("F2")}x",      $"From Buffs: {localPlayer.CalculateTotalMultiplierFromBuffs(StatModifierKind.MouthSize).ToString("F2")}x");
            DoSingleStatUI(ref statsRect, "stomach", statsTextSettings, $"Stomach Level: {localPlayer.StomachSizeLevel}",    $"Stomach Size: {localPlayer.ModifiedStomachSize.ToString("F2")}",        $"Base: {localPlayer.BaseStomachSizeValue.ToString("F2")}",    $"From Pets: {localPlayer.CalculateTotalMultiplierFromPets(StatModifierKind.StomachSize).ToString("F2")}x",    $"From Buffs: {localPlayer.CalculateTotalMultiplierFromBuffs(StatModifierKind.StomachSize).ToString("F2")}x");
            DoSingleStatUI(ref statsRect, "click",   statsTextSettings, $"Click Power Level: {localPlayer.ClickPowerLevel}", $"Click Power: {localPlayer.ModifiedClickPower.ToString("F2")}x",         $"Base: {localPlayer.BaseClickPowerValue.ToString("F2")}",     $"From Pets: {localPlayer.CalculateTotalMultiplierFromPets(StatModifierKind.ClickPower).ToString("F2")}x",     $"From Buffs: {localPlayer.CalculateTotalMultiplierFromBuffs(StatModifierKind.ClickPower).ToString("F2")}x");
            DoSingleStatUI(ref statsRect, "rebirth", statsTextSettings, $"Rebirth Level: {localPlayer.Rebirth}",             $"Cash Multiplier: {localPlayer.ModifiedCashMultiplier.ToString("F2")}x", $"Base: {localPlayer.BaseCashMultiplierValue.ToString("F2")}", $"From Pets: {localPlayer.CalculateTotalMultiplierFromPets(StatModifierKind.Money).ToString("F2")}x",          $"From Buffs: {localPlayer.CalculateTotalMultiplierFromBuffs(StatModifierKind.Money).ToString("F2")}x");
        }

        var buttonSettings = new UI.ButtonSettings();
        buttonSettings.color = Vector4.White;
        buttonSettings.clickedColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
        buttonSettings.hoverColor   = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
        buttonSettings.pressedColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        buttonSettings.sprite = References.Instance.GreenButton;
        buttonSettings.slice = References.Instance.ButtonSlice;

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

        var sideBarRect = UI.SafeRect.LeftRect().GrowRight(100).Inset(200, 0, 0, 0).Offset(15, 0);
        var sidebarGrid = UI.GridLayout.Make(sideBarRect, 100, 100, UI.GridLayout.SizeSource.ELEMENT_SIZE);

        bool drawSidebarButton(String text, Texture icon) 
        {
            var sidebarButtonTextSettings = new UI.TextSettings() 
            {
                font = UI.TextSettings.Font.AlphaKind,
                size = 26,
                color = Vector4.White,
                horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
                wordWrap = false,
                outline = true,
                outlineThickness = 3,
            };

            var sidebarButtonSettings = new UI.ButtonSettings()
            {
                color = Vector4.White,
                clickedColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f),
                hoverColor   = new Vector4(0.9f, 0.9f, 0.9f, 1.0f),
                pressedColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            };

            using var _1 = UI.PUSH_ID(text);
            var buttonRect = sidebarGrid.Next().Inset(7);
            var buttonResult = UI.Button(buttonRect, text, sidebarButtonSettings, sidebarButtonTextSettings);
            using var _ = UI.PUSH_COLOR(buttonResult.ColorMultiplier);
            UI.PushLayerRelative(-1);
            using var _2 = AllOut.Defer(UI.PopLayer);
            UI.Image(buttonRect, References.Instance.MenuIcon, Vector4.White);
            UI.Image(buttonRect.Inset(10), icon, Vector4.White);
            return buttonResult.clicked;
        }

        // if (drawSidebarButton("Store", References.Instance.Shop)) 
        // {
        //     IsShowingShopWindow = !IsShowingShopWindow;
        // }

        if (drawSidebarButton("Upgrade", References.Instance.Upgrade)) 
        {
            IsShowingUpgradesWindow = !IsShowingUpgradesWindow;
        }

        if (drawSidebarButton("Rebirth", References.Instance.Rebirth)) 
        {
            IsShowingRebirthWindow = !IsShowingRebirthWindow;
        }

        if (drawSidebarButton("Pets", References.Instance.PetBrown)) 
        {
            IsShowingPetsWindow = !IsShowingPetsWindow;
        }

        // if (drawSidebarButton("Stats", References.Instance.Stats)) 
        // {

        // }

        UI.PopId();

        if (IsShowingUpgradesWindow) 
        {
            UI.PushId("UPGRADES_WINDOW");
            using var _1 = AllOut.Defer(UI.PopId);

            if (UI.Button(UI.ScreenRect, "CLOSE_UPGRADES_WINDOW", new UI.ButtonSettings(), new UI.TextSettings()).clicked) 
            {
                IsShowingUpgradesWindow = false;
            }

            var windowRect = UI.SafeRect.CenterRect();
            windowRect = windowRect.Grow(350, 550, 350, 550);
            UI.Blocker(windowRect, "upgrades");
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);

            var iconRect = windowRect.TopLeftRect().Grow(40, 40, 40, 40).Offset(0, -5);
            UI.Image(iconRect, References.Instance.Upgrade, Vector4.White);
            var textRect = iconRect.CenterRect().Grow(25, 0, 25, 0).Offset(25, 0);
            UI.Text(textRect, "Upgrades", new UI.TextSettings(){
                color = References.Instance.YellowText,
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
                IsShowingUpgradesWindow = false;
            }

            var gridRect = windowRect.Inset(100, 75, 100, 75);
            var grid = UI.GridLayout.Make(gridRect, 1, 3, UI.GridLayout.SizeSource.GRID_SIZE);
            
            bool drawStatUpgrade(string title, string description, double cost, Texture icon)
            {
                using var _ = UI.PUSH_ID(title);

                var upgradeRect = grid.Next().Inset(10);
                UI.Image(upgradeRect, References.Instance.FrameWhite, new Vector4(233.0f/255.0f, 233.0f/255.0f, 233.0f/255.0f, 1.0f), References.Instance.WhiteFrameSlice);
                
                var iconRect = upgradeRect.LeftRect().GrowRightUnscaled(upgradeRect.Height).Inset(10);
                UI.Image(iconRect, icon, Vector4.White);

                var titleRect = iconRect.TopRightRect().Offset(10, -5);
                var realTitleRect = UI.Text(titleRect, title, new UI.TextSettings(){
                    color = Vector4.Black,
                    font = UI.TextSettings.Font.AlphaKind,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Top,
                    size = 42,
                });

                var descriptionRect = realTitleRect.BottomLeftRect().GrowRight(350).Offset(0, -3);
                UI.Text(descriptionRect, description, new UI.TextSettings(){
                    color = Vector4.Black,
                    font = UI.TextSettings.Font.AlphaKind,
                    wordWrap = true,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Top,
                    size = 26,
                });

                var buttonRect = upgradeRect.RightRect().GrowLeftUnscaled(250).Inset(13);
                var bts = new UI.TextSettings()
                {
                    color = Vector4.White,
                    outline = true,
                    outlineThickness = 2,
                    font = UI.TextSettings.Font.AlphaKind,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    offset = new Vector2(0, 18),
                    size = 42,
                };
                var upgradeStomachResult = UI.Button(buttonRect, $"Upgrade!", buttonSettings, bts);

                {
                    using var _1 = UI.PUSH_COLOR(upgradeStomachResult.ColorMultiplier);

                    var costTextRect = buttonRect.BottomRect().Offset(0, 15).Offset(15, 0);
                    var realCostTextRect = UI.Text(costTextRect, $"{cost}", new UI.TextSettings(){
                        color = References.Instance.YellowText,
                        font = UI.TextSettings.Font.AlphaKind,
                        outline = true,
                        outlineThickness = 2,
                        horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                        verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
                        size = 30,
                    });
                    UI.Image(realCostTextRect.LeftRect().GrowLeftUnscaled(realCostTextRect.Height).Offset(-2, 0), References.Instance.CoinIcon, Vector4.White);
                }

                return upgradeStomachResult.clicked;
            }
            

            
            double nextMouthSizeCost = -1;
            if (localPlayer.MouthSizeLevel < FatPlayer.MouthSizeByLevel.Length)
                nextMouthSizeCost = FatPlayer.MouthSizeByLevel[localPlayer.MouthSizeLevel].Cost;
            if (drawStatUpgrade("Mouth Size", "Fit larger items in your mouth", nextMouthSizeCost, References.Instance.MouthSizeIcon)) 
            {
                localPlayer.CallServer_RequestPurchaseMouthSize();
            }

            double nextStomachSizeCost = -1;
            if (localPlayer.StomachSizeLevel < FatPlayer.StomachSizeByLevel.Length)
                nextStomachSizeCost = FatPlayer.StomachSizeByLevel[localPlayer.StomachSizeLevel].Cost;
            if (drawStatUpgrade("Stomach Size", "Fit more food in your stomach", nextStomachSizeCost, References.Instance.StomachSizeIcon))
            {
                localPlayer.CallServer_RequestPurchaseStomachSize();
            }

            double nextClickPowerCost = -1;
            if (localPlayer.ClickPowerLevel < FatPlayer.ClickPowerByLevel.Length)
                nextClickPowerCost = FatPlayer.ClickPowerByLevel[localPlayer.ClickPowerLevel].Cost;
            if (drawStatUpgrade("Click Power", "Eat more food with each click", nextClickPowerCost, References.Instance.ChewSpeedIcon))
            {
                localPlayer.CallServer_RequestPurchaseClickPower();
            }
        }

        if (IsShowingShopWindow) 
        {
            UI.PushId("SHOP_WINDOW");
            using var _1 = AllOut.Defer(UI.PopId);

            IsShowingShopWindow = Shop.Instance.DrawShop();
        }

        HoveredPet = null;
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
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);

            var equippedPetsCount = localPlayer.PetManager.OwnedPets.Count(p => p.Equipped);
            // title/top row
            {
                UI.PushLayerRelative(1); using var _2 = AllOut.Defer(UI.PopLayer);

                var iconRect = windowRect.TopLeftRect().Grow(40, 40, 40, 40).Offset(0, -5);
                UI.Image(iconRect, References.Instance.PetBrown, Vector4.White);
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
                UI.Image(equippedCapacityRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                var equippedCapacityIconRect = equippedCapacityRect.LeftRect().Grow(0, equippedCapacityRectHeight/2, 0, equippedCapacityRectHeight/2).Grow(10, 10, 10, 10);
                UI.Image(equippedCapacityIconRect, References.Instance.Backpack, Vector4.White);
                var equippedCapacityTextRect = equippedCapacityRect.LeftRect().Offset(85, 0);
                UI.Text(equippedCapacityTextRect, $"{equippedPetsCount}/{localPlayer.MaxEquippedPets}", new UI.TextSettings(){
                    color = References.Instance.YellowText,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Left,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36,
                });
                var increaseEquippedCapacityRect = equippedCapacityRect.RightRect().Grow(0, equippedCapacityRectHeight/2, 0, equippedCapacityRectHeight/2);
                increaseEquippedCapacityRect = increaseEquippedCapacityRect.Offset(-equippedCapacityRectHeight/2, 0).Inset(5, 10, 5, 0);
                if (UI.Button(increaseEquippedCapacityRect.FitAspect(References.Instance.Plus.Aspect), "increase_equipped_capacity", new UI.ButtonSettings(){ sprite = References.Instance.Plus }, new UI.TextSettings(){size = 0, color = Vector4.Zero}).clicked)
                {
                    IsShowingPetsWindow = false;
                    IsShowingShopWindow = true;
                    Shop.Instance.ScrollToIAP = "pet_equip_cap_3";
                }
            }


            if (SelectedPet != null)
            {
                var petDefn = SelectedPet.GetDefinition();

                var selectedPetRect = windowRect.CutRight(500).Inset(75, 0, 75, 0);
                var dividerRect = selectedPetRect.CutLeft(2);
                UI.Image(dividerRect, null, Vector4.Black * 0.25f);
                
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
                var petIconRect = infoRect.CutLeftUnscaled(infoRect.Width*0.5f);
                petIconRect = petIconRect.Inset(10, 10, 10, 10).Offset(10, 0);
                UI.Image(petIconRect.FitAspect(petDefn.IconSprite.Aspect), petDefn.IconSprite, Vector4.White);

                var rarityColor = GetRarityColor(petDefn.Rarity);
                var rarityRect = infoRect.CutTop(50);
                UI.Text(rarityRect, petDefn.Rarity.ToString(), new UI.TextSettings(){
                    color = rarityColor,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36,
                });
                
                infoRect.CutTop(5);
                foreach(var modifier in petDefn.StatModifiers)
                {
                    var statModRect = infoRect.CutTop(50);
                    var (text, col) = BuildStatModifierText(modifier);
                    
                    UI.Text(statModRect, text, new UI.TextSettings() {
                        color = col,
                        outline = true,
                        outlineThickness = 2,
                        horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                        verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
                        size = 24
                    });
                }
                var buttonTs = new UI.TextSettings() {
                    color = Vector4.White,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36
                };
                var equipButtonRect = selectedPetRect.CutTop(90).Inset(5, 10, 5, 10);
                if (SelectedPet.Equipped) 
                {
                    var unequipButtonResult = UI.Button(equipButtonRect, "Unequip", new UI.ButtonSettings(){ sprite = References.Instance.BlueButton, slice = References.Instance.WhiteFrameSlice }, buttonTs);
                    if (unequipButtonResult.clicked) 
                    {
                        localPlayer.CallServer_RequestUnequipPet(SelectedPet.Id);
                    }
                }
                else 
                {
                    if (equippedPetsCount >= localPlayer.MaxEquippedPets)
                    {
                        UI.PushDisabled();
                    }
                    var equipButtonResult = UI.Button(equipButtonRect, "Equip", new UI.ButtonSettings(){ sprite = References.Instance.GreenButton, slice = References.Instance.WhiteFrameSlice }, buttonTs);
                    if (equipButtonResult.clicked) 
                    {
                        localPlayer.CallServer_RequestEquipPet(SelectedPet.Id);
                    }
                    if (equippedPetsCount >= localPlayer.MaxEquippedPets) {
                        UI.PopDisabled();
                    }
                }

                var deleteButtonRect = selectedPetRect.CutTop(90).Inset(5, 10, 5, 10);
                var deleteButtonResult = UI.Button(deleteButtonRect, "Delete", new UI.ButtonSettings(){ sprite = References.Instance.RedButton, slice = References.Instance.WhiteFrameSlice }, buttonTs);
                if (deleteButtonResult.clicked) 
                {
                    PetToDelete = SelectedPet;
                    // localPlayer.CallServer_RequestDeletePet(SelectedPet.Id);
                }
            }

            {
                Rect contentRect = windowRect.Inset(5, 50, 5, 50);
                var scrollView = UI.PushScrollView("pets_scroll_view", contentRect, new UI.ScrollViewSettings() { Vertical = true, });
                using var _3 = AllOut.Defer(() => UI.PopScrollView());

                var petsRect = scrollView.contentRect.TopRect().Offset(0, -25);
                var grid = UI.GridLayout.Make(petsRect, 165, 165, UI.GridLayout.SizeSource.ELEMENT_SIZE);

                int petCount = Math.Min(localPlayer.MaxPetsInStorage, localPlayer.PetManager.OwnedPets.Count);
                for (int i = 0; i < petCount; i++)
                {
                    var pet = localPlayer.PetManager.OwnedPets[i];
                    bool isSelected = SelectedPet == pet;
                    UI.PushId(pet.Id);
                    using var _2 = AllOut.Defer(UI.PopId);

                    var petRect = grid.Next();
                    petRect = petRect.Inset(5);
                    buttonSettings.sprite = References.Instance.FrameWhite;

                    if (isSelected) {
                        UI.Image(petRect.Grow(2), References.Instance.PanelContent, Vector4.Green, References.Instance.WhiteFrameSlice);
                    }

                    var petDefn = pet.GetDefinition();
                    buttonSettings.colorMultiplier = petDefn.Rarity switch
                    {
                        PetData.Rarity.Uncommon => RarityColorUncommon,
                        PetData.Rarity.Rare => RarityColorRare,
                        PetData.Rarity.Epic => RarityColorEpic,
                        PetData.Rarity.Legendary => RarityColorLegendary,
                        _ => RarityColorCommon,
                    };
                    
                    var petButtonResult = UI.Button(petRect, pet.Name, buttonSettings, References.Instance.NoTextSettings);
                    if (petButtonResult.clicked) {
                        SelectedPet = isSelected ? null : pet;
                        SelectedPetDisplayT = 0;
                    }

                    if (petButtonResult.hovering) {
                        HoveredPet = pet;
                    }

                    var petIconRect = petRect.Inset(10);
                    UI.Image(petIconRect.FitAspect(petDefn.IconSprite.Aspect), petDefn.IconSprite, Vector4.White);

                    if (pet.Equipped)
                    {
                        var checkmarkRect = petRect.BottomLeftRect().Grow(15).Offset(25, 25);
                        UI.Image(checkmarkRect, References.Instance.CheckMark, Vector4.White);
                    }
                }

                int petsInColdStorage = localPlayer.PetManager.OwnedPets.Count - petCount;
                if (petsInColdStorage > 0)
                {
                    UI.PushId("coldstorage");
                    using var _ = AllOut.Defer(UI.PopId);

                    var petRect = grid.Next();
                    petRect = petRect.Inset(5);
                    buttonSettings.sprite = References.Instance.FrameWhite;

                    buttonSettings.colorMultiplier = RarityColorCommon;
                    var coldStorageTextSettings = new UI.TextSettings()
                    {
                        font = UI.TextSettings.Font.AlphaKind,
                        size = 40,
                        color = Vector4.White,
                        horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                        verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                        wordWrap = true,
                        outline = true,
                        // dropShadow = false,
                    };

                    var petButtonResult = UI.Button(petRect, $"+{petsInColdStorage} in storage", buttonSettings, coldStorageTextSettings);
                    if (petButtonResult.clicked)
                    {
                        IsShowingPetsWindow = false;
                        IsShowingShopWindow = true;
                        Shop.Instance.ScrollToIAP = "pet_storage_cap_1";
                    }
                }

                // Make an extra row for buffer at then end of the grid
                for (var j = 0; j < 6; j++) 
                {
                    var petRect = grid.Next();
                    UI.ExpandCurrentScrollView(petRect);
                }
            }

            if (PetToDelete != null)
            {
                UI.PushId("DELETE_PET_WINDOW");
                using var _4 = AllOut.Defer(UI.PopId);

                UI.Image(UI.ScreenRect, null, Vector4.Black * 0.5f);
                if (UI.Button(UI.ScreenRect, "CLOSE_DELETE_PET_WINDOW", new UI.ButtonSettings(), new UI.TextSettings()).clicked) 
                {
                    PetToDelete = null;
                }

                var deleteRect = UI.SafeRect.CenterRect().Grow(150, 200, 150, 200);
                UI.Image(deleteRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                var deleteTextRect = deleteRect.CutTop(150);
                UI.Text(deleteTextRect, $"Are you sure you want to delete {PetToDelete.Name}?", new UI.TextSettings(){
                    color = Vector4.White,
                    outline = true,
                    outlineThickness = 2,
                    wordWrap = true,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 32,
                });

                var deleteButtonRect = deleteRect.CutTop(75).Inset(0, 10, 10, 10);
                var deleteButtonResult = UI.Button(deleteButtonRect, "Delete", new UI.ButtonSettings(){ sprite = References.Instance.RedButton, slice = References.Instance.WhiteFrameSlice }, new UI.TextSettings(){
                    color = Vector4.White,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36,
                });
                if (deleteButtonResult.clicked) 
                {
                    localPlayer.CallServer_RequestDeletePet(PetToDelete.Id);
                    PetToDelete = null;
                    SelectedPet = null;
                }

                var cancelButtonRect = deleteRect.CutTop(75).Inset(0, 10, 10, 10);
                var cancelButtonResult = UI.Button(cancelButtonRect, "Cancel", new UI.ButtonSettings(){ sprite = References.Instance.GreenButton, slice = References.Instance.WhiteFrameSlice }, new UI.TextSettings(){
                    color = Vector4.White,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36,
                });
                if (cancelButtonResult.clicked) 
                {
                    PetToDelete = null;
                }
            }
            else if (HoveredPet != null)
            {
                var rectSize = HoveredPet.GetDefinition().StatModifiers.Count * 40 + (50 * 2) + 10;

                var pos = Input.GetMouseScreenPosition();
                Rect tooltipRect = new Rect(pos).Grow(0, 200, rectSize, 0).Offset(20, -20);
                UI.Image(tooltipRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);

                var nameRect = tooltipRect.CutTop(50);
                UI.Text(nameRect, HoveredPet.Name, new UI.TextSettings(){
                    color = Vector4.White,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 36,
                });

                var rarityRect = tooltipRect.CutTop(40);
                var rarityColor = HoveredPet.GetDefinition().Rarity switch
                {
                    PetData.Rarity.Uncommon => RarityColorUncommon,
                    PetData.Rarity.Rare => RarityColorRare,
                    PetData.Rarity.Epic => RarityColorEpic,
                    PetData.Rarity.Legendary => RarityColorLegendary,
                    _ => RarityColorCommon,
                };
                UI.Text(rarityRect, HoveredPet.GetDefinition().Rarity.ToString(), new UI.TextSettings(){
                    color = rarityColor,
                    outline = true,
                    outlineThickness = 2,
                    horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                    verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                    size = 32,
                });
                
                tooltipRect.CutTop(3);
                var dividerRect = tooltipRect.CutTop(2).Inset(0, 10, 0, 10);
                UI.Image(dividerRect, null, Vector4.Black * 0.25f);

                foreach(var modifier in HoveredPet.GetDefinition().StatModifiers)
                {
                    var statModRect = tooltipRect.CutTop(35);
                    var (text, col) = BuildStatModifierText(modifier);
                    UI.Text(statModRect, text, new UI.TextSettings() {
                        color = col,
                        outline = true,
                        outlineThickness = 2,
                        horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                        verticalAlignment = UI.TextSettings.VerticalAlignment.Bottom,
                        size = 24
                    });
                }
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
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);

            var halfWidth = windowRect.Width / 2;

            var currentRebirthData = Rebirth.Instance.GetRebirthData(localPlayer.Rebirth);
            var nextRebirthData    = Rebirth.Instance.GetRebirthData(localPlayer.Rebirth + 1);

            // Current
            {
                var beforeRect = windowRect.LeftRect().GrowRightUnscaled(halfWidth).Inset(50, 200, 225, 50);
                UI.Image(beforeRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                
                beforeRect = beforeRect.SubRect(0, 0, 1, 1, 0, 0, 10, 0);
                var h = beforeRect.Height / 5;

                var textRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                headerTextSettings.color = Vector4.White;
                UI.Text(textRect, "Current:", headerTextSettings);

                var trophiesRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(trophiesRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(trophiesRect, $"{Util.FormatDouble(localPlayer.Trophies)}", upgradeItemTextSettings);

                var cashRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.GreenText;
                UI.Text(cashRect, $"{Util.FormatDouble(localPlayer.Coins)}", upgradeItemTextSettings);

                var cashMultiplierRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashMultiplierRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.BlueText;
                UI.Text(cashMultiplierRect, $"{currentRebirthData.CashMultiplier:0.##}", upgradeItemTextSettings);

                var rankRect = beforeRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(rankRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(rankRect, $"{currentRebirthData.RankName}", upgradeItemTextSettings);
            }
            
            // Next
            {
                var afterRect = windowRect.RightRect().GrowLeftUnscaled(halfWidth).Inset(50, 50, 225, 200);
                UI.Image(afterRect, References.Instance.FrameWhite, References.Instance.BlueBg, References.Instance.WhiteFrameSlice);

                afterRect = afterRect.SubRect(0, 0, 1, 1, 0, 0, 10, 0);
                var h = afterRect.Height / 5;

                var textRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                headerTextSettings.color = References.Instance.YellowText;
                UI.Text(textRect, "Next:", headerTextSettings);

                var trophiesRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(trophiesRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(trophiesRect, $"{Util.FormatDouble(localPlayer.Trophies - nextRebirthData.TrophiesCost)}", upgradeItemTextSettings);

                var cashRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.GreenText;
                UI.Text(cashRect, $"0", upgradeItemTextSettings);

                var cashMultiplierRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(cashMultiplierRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.BlueText;
                UI.Text(cashMultiplierRect, $"{nextRebirthData.CashMultiplier:0.##}", upgradeItemTextSettings);

                var rankRect = afterRect.CutTopUnscaled(h).Inset(10, 20, 10, 20);
                UI.Image(rankRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);
                upgradeItemTextSettings.color = References.Instance.YellowText;
                UI.Text(rankRect, $"{nextRebirthData.RankName}", upgradeItemTextSettings);
            }

            {
                var bottomRect = windowRect.BottomRect()
                                           .GrowTopUnscaled(75)
                                           .Offset(0, 50)
                                           .Inset(0, 50, 0, 50);

                var sliderRect = bottomRect.CutLeftUnscaled((bottomRect.Width / 3) * 2);
                UI.Image(sliderRect, References.Instance.FrameWhite, Vector4.White, References.Instance.WhiteFrameSlice);

                double has   = localPlayer.Trophies;
                double needs = currentRebirthData.TrophiesCost;

                var rebirthProgress = Math.Clamp(has / needs, 0, 1);
                var rebirthProgressRect = sliderRect.Inset(5).SubRect(0, 0, (float)rebirthProgress, 1, 0, 0, 0, 0);
                UI.Image(rebirthProgressRect, References.Instance.BlueFill, Vector4.White);
                UI.Text(sliderRect, $"{Util.FormatDouble(localPlayer.Trophies)} / {Util.FormatDouble(nextRebirthData.TrophiesCost)}", upgradeItemTextSettings);

                var confirmRebirthButtonRect = bottomRect.CutLeftUnscaled(bottomRect.Width * 0.6f).Inset(0, 25, 0, 15);
                if (UI.Button(confirmRebirthButtonRect, "Rebirth", buttonSettings, buttonTextSettings).clicked)
                {
                    localPlayer.CallServer_RequestRebirth();
                }

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
            var playerRectPos = Camera.WorldToScreen(localPlayer.Entity.Position - new Vector2(0, 0.5f));
            var myRect = new Rect(playerRectPos, playerRectPos).Grow(25, 100, 25, 100);

            var bossRectPos = Camera.WorldToScreen(localPlayer.CurrentBoss.Entity.Position - new Vector2(0, 0.5f));
            var bossRect = new Rect(bossRectPos, bossRectPos).Grow(25, 100, 25, 100);

            double myProgress   = AOMath.Lerp(0.25f, 1.0f, (float)Math.Min(1.0, (double)localPlayer.MyProgress   / (double)localPlayer.CurrentBoss.AmountToWin));
            double bossProgress = AOMath.Lerp(0.25f, 1.0f, (float)Math.Min(1.0, (double)localPlayer.BossProgress / (double)localPlayer.CurrentBoss.AmountToWin));

            var myProgressRect   = myRect  .SubRect(0, 0, (float)myProgress,   1, 0, 0, 0, 0);
            var bossProgressRect = bossRect.SubRect(0, 0, (float)bossProgress, 1, 0, 0, 0, 0);

            UI.Image(myRect, References.Instance.BossBarBg, Vector4.White, References.Instance.BossBarSlice);
            UI.Image(myProgressRect, References.Instance.BossBarBg, Vector4.Green, References.Instance.BossBarSlice);
            UI.Text(myRect, $"{localPlayer.MyProgress}", textSettings);

            UI.Image(bossRect, References.Instance.BossBarBg, Vector4.White, References.Instance.BossBarSlice);
            UI.Image(bossProgressRect, References.Instance.BossBarBg, Vector4.Red, References.Instance.BossBarSlice);
            UI.Text(bossRect, $"{localPlayer.BossProgress}", textSettings);
        }
    }

    public override void Shutdown()
    {
    }
}
