
using System.Globalization;
using AO;

public class GameUI : System<GameUI> 
{
    public bool IsShowingUpgradesWindow;
    public bool IsShowingShopWindow;
    public bool IsShowingPetsWindow;

    public override void Start()
    {
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
            color = Vector4.Black,
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Right,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
            wordWrap = false,
            wordWrapOffset = 0,
            offset = new Vector2(0, 7),
            // dropShadow = false,
            // outline= false,
        };
        
        var coinsRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(coinsRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
        var coinsIconRect = coinsRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3).Offset(6, 0);
        UI.Image(coinsIconRect, References.Instance.CoinIcon, Vector4.White, new UI.NineSlice());
        UI.Text(coinsRect, $"{localPlayer.Coins}", textSettings);
        
        var foodRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(foodRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
        var foodIconRect = foodRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3).Offset(6, 0);
        UI.Image(foodIconRect, References.Instance.FoodIcon, Vector4.White, new UI.NineSlice());
        UI.Text(foodRect, $"{localPlayer.Food}/{localPlayer.MaxFood}", textSettings);
        
        var mouthSizeRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(mouthSizeRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
        var mouthSizeIconRect = mouthSizeRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3).Offset(6, 0);
        UI.Image(mouthSizeIconRect, References.Instance.MouthSizeIcon, Vector4.White, new UI.NineSlice());
        UI.Text(mouthSizeRect, $"{localPlayer.MouthSize}", textSettings);
        
        var chewSpeedRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(chewSpeedRect, References.Instance.FrameWhite, Vector4.White, References.Instance.FrameSlice);
        var chewSpeedIconRect = chewSpeedRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3).Offset(6, 0);
        UI.Image(chewSpeedIconRect, References.Instance.ChewSpeedIcon, Vector4.White, new UI.NineSlice());
        UI.Text(chewSpeedRect, $"{localPlayer.ChewSpeed}", textSettings);


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
            horizontalAlignment = UI.TextSettings.HorizontalAlignment.Right,
            verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
            wordWrap = false,
            wordWrapOffset = 0,
            offset = new Vector2(0, 10),
            // dropShadow = false,
            // outline= false,
        };
    
        var upgradesButtonRect = sideBarRect.CutTop(100).Inset(10, 10, 10, 10);
        var upgradesButtonResult = UI.Button(upgradesButtonRect, "Upgrades", buttonSettings, buttonTextSettings);
        if (upgradesButtonResult.clicked) {
            IsShowingUpgradesWindow = !IsShowingUpgradesWindow;
        }

        var shopButtonRect = sideBarRect.CutTop(100).Inset(10, 10, 10, 10);
        var shopButtonResult = UI.Button(shopButtonRect, "Shop", buttonSettings, buttonTextSettings);
        if (shopButtonResult.clicked) {
            IsShowingShopWindow = !IsShowingShopWindow;
        }

        var petsButtonRect = sideBarRect.CutTop(100).Inset(10, 10, 10, 10);
        var petsButtonResult = UI.Button(petsButtonRect, "Pets", buttonSettings, buttonTextSettings);
        if (petsButtonResult.clicked) {
            IsShowingPetsWindow = !IsShowingPetsWindow;
        }

        if (IsShowingUpgradesWindow) {
            if (UI.Button(UI.ScreenRect, "", new UI.ButtonSettings(), new UI.TextSettings()).clicked) {
                IsShowingUpgradesWindow = false;
            }

            var windowRect = UI.SafeRect.CenterRect();
            windowRect = windowRect.Grow(200, 300, 200, 300);
            UI.Image(windowRect, References.Instance.WindowBg, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});
            
            var upgradeStomachRect = windowRect.CutTopUnscaled(100).Inset(10, 10, 10, 10);
            var upgradeStomachResult = UI.Button(upgradeStomachRect, "Stomach", buttonSettings, buttonTextSettings);
            if (upgradeStomachResult.clicked) {
                localPlayer.CallServer_RequestPurchaseStoachSize();
            }

            var upgradeMouthSizeRect = windowRect.CutTopUnscaled(100).Inset(10, 10, 10, 10);
            var upgradeMouthSizeResult = UI.Button(upgradeMouthSizeRect, "Mouth Size", buttonSettings, buttonTextSettings);
            if (upgradeMouthSizeResult.clicked) {
                localPlayer.CallServer_RequestPurchaseMouthSize();
            }

            var upgradeChewSpeedRect = windowRect.CutTopUnscaled(100).Inset(10, 10, 10, 10);
            var upgradeChewSpeedResult = UI.Button(upgradeChewSpeedRect, "Chew Speed", buttonSettings, buttonTextSettings);
            if (upgradeChewSpeedResult.clicked) {
                localPlayer.CallServer_RequestPurchaseChewSpeed();
            }
        }

        if (IsShowingShopWindow) {
            IsShowingShopWindow = Shop.Instance.DrawShop();
        }

        if (IsShowingPetsWindow) {
            if (UI.Button(UI.ScreenRect, "", new UI.ButtonSettings(), new UI.TextSettings()).clicked) {
                IsShowingPetsWindow = false;
            }

            var windowRect = UI.SafeRect.CenterRect();
            windowRect = windowRect.Grow(200, 300, 200, 300);
            UI.Image(windowRect, References.Instance.WindowBg, Vector4.White, new UI.NineSlice(){ slice = new Vector4(20, 20, 50, 50), sliceScale = 1f});

            foreach (var pet in localPlayer.PetManager.OwnedPets) {
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
    }

    public override void Shutdown()
    {
    }
}
