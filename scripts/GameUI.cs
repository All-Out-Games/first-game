
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

    public override void Update()
    {
        if (Network.IsServer) return;

        var localPlayer = (FatPlayer) Network.LocalPlayer;
        if (localPlayer == null) return;

        Rect topBarRect = UI.SafeRect.TopRect();
        topBarRect = topBarRect.GrowBottom(65)
                               .Inset(0, 450, 0, 200)
                               .Offset(0, -5);
        var w = topBarRect.Width / 4;

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
            outlineThickenss = 2,
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
            windowRect = windowRect.Grow(200, 300, 200, 300);
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});

            foreach (var pet in localPlayer.PetManager.OwnedPets) 
            {
                UI.PushId(pet.Id);
                using var _2 = AllOut.Defer(UI.PopId);

                var petRect = windowRect.CutTopUnscaled(100).Inset(10, 10, 10, 10);
                buttonSettings.sprite = pet.Equipped ? References.Instance.GreenButton : References.Instance.RedButton;
                var petButtonResult = UI.Button(petRect, pet.Name, buttonSettings, buttonTextSettings);
                if (petButtonResult.clicked) {
                    if (pet.Equipped)
                    {
                        localPlayer.CallServer_RequestUnequipPet(pet.Id);
                    }
                    else 
                    {
                        localPlayer.CallServer_RequestEquipPet(pet.Id);
                    }
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
            UI.Image(windowRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);

            var halfWidth = windowRect.Width / 2;

            var currentRebirthData = Rebirth.Instance.GetRebirthData(localPlayer.Rebirth);
            var nextRebirthData    = Rebirth.Instance.GetRebirthData(localPlayer.Rebirth);

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
