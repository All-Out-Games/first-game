
using AO;

public class GameUI : System<GameUI> 
{
    public override void Start()
    {
    }

    public override void Update()
    {
        if (Network.IsServer) return;

        Rect topBarRect = UI.SafeRect.TopRect();
        topBarRect = topBarRect.GrowBottom(50);

        UI.Image(topBarRect, null, Vector4.Red, new UI.NineSlice());
    }

    public override void Shutdown()
    {
    }
}
