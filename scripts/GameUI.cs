
using AO;

public class GameUI : System<GameUI> 
{
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
            offset = Vector2.Zero,
            // dropShadow = false,
            // outline= false,
        };
        
        var coinsRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(coinsRect, null, Vector4.White, new UI.NineSlice());
        var coinsIconRect = coinsRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3);
        UI.Image(coinsIconRect, References.Instance.CoinIcon, Vector4.White, new UI.NineSlice());
        UI.Text(coinsRect, $"{localPlayer.Coins}", textSettings);
        
        var foodRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(foodRect, null, Vector4.White, new UI.NineSlice());
        var foodIconRect = foodRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3);
        UI.Image(foodIconRect, References.Instance.FoodIcon, Vector4.White, new UI.NineSlice());
        UI.Text(foodRect, $"{localPlayer.Food}/{localPlayer.MaxFood}", textSettings);
        
        var mouthSizeRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(mouthSizeRect, null, Vector4.White, new UI.NineSlice());
        var mouthSizeIconRect = mouthSizeRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3);
        UI.Image(mouthSizeIconRect, References.Instance.MouthSizeIcon, Vector4.White, new UI.NineSlice());
        UI.Text(mouthSizeRect, $"{localPlayer.MouthSize}", textSettings);
        
        var chewSpeedRect = topBarRect.CutLeftUnscaled(w).Inset(0, 10, 0, 10);
        UI.Image(chewSpeedRect, null, Vector4.White, new UI.NineSlice());
        var chewSpeedIconRect = chewSpeedRect.Copy().CutLeft(50).FitAspect(1).Inset(3, 3, 3, 3);
        UI.Image(chewSpeedIconRect, References.Instance.ChewSpeedIcon, Vector4.White, new UI.NineSlice());
        UI.Text(chewSpeedRect, $"{localPlayer.ChewSpeed}", textSettings);
    }

    public override void Shutdown()
    {
    }
}
