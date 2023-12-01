using AO;

public class References : Component
{
    [Serialized] public Texture CoinIcon;
    [Serialized] public Texture FoodIcon;
    [Serialized] public Texture MouthSizeIcon;
    [Serialized] public Texture ChewSpeedIcon;

    [Serialized] public Texture GreenButton;
    [Serialized] public Texture RedButton;
    [Serialized] public Texture BlueButton;
    [Serialized] public Texture OrangeButton;
    [Serialized] public Texture GreenFill;
    [Serialized] public Texture RedFill;
    [Serialized] public Texture BlueFill;
    [Serialized] public Texture OrangeFill;
    [Serialized] public Texture FrameDark;
    [Serialized] public Texture FrameWhite;
    [Serialized] public Texture PanelContent;
    [Serialized] public Texture WindowBg;

    public UI.NineSlice FrameSlice = new UI.NineSlice() { slice = new Vector4(12, 12, 48, 48), sliceScale = 1f };
    

    [Serialized] public Prefab PetPrefab;

    public static References Instance;

    public override void Start()
    {
        Instance = this;
    }
}