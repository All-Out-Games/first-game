using AO;

public class References : Component
{
    [Serialized] public Texture CoinIcon;
    [Serialized] public Texture TrophyIcon;
    [Serialized] public Texture FoodIcon;

    [Serialized] public Texture MouthSizeIcon;
    [Serialized] public Texture StomachSizeIcon;
    [Serialized] public Texture ChewSpeedIcon;

    [Serialized] public Texture MenuIcon;
    [Serialized] public Texture TopBarBg;

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
    [Serialized] public Texture PanelBackground;

    [Serialized] public Texture Backpack;
    [Serialized] public Texture X;
    [Serialized] public Texture CheckMark;
    [Serialized] public Texture Plus;
    [Serialized] public Texture Trophy;
    [Serialized] public Texture Upgrade;
    [Serialized] public Texture Shop;
    [Serialized] public Texture Stats;
    [Serialized] public Texture Rebirth;
    [Serialized] public Texture PetBrown;
    [Serialized] public Texture Cash;
    [Serialized] public Texture Burger;

    [Serialized] public Prefab CarePackagePrefab;
    [Serialized] public Prefab PetPrefab;

    [Serialized] public SpineSkeleton IceCreamConePet;
    [Serialized] public SpineSkeleton BurgerPet;
    [Serialized] public SpineSkeleton DrumstickPet;
    [Serialized] public SpineSkeleton CarrotPet;
    [Serialized] public SpineSkeleton PenguinPet;
    [Serialized] public SpineSkeleton DiamondPet;
    [Serialized] public SpineSkeleton FriesPet;
    [Serialized] public SpineSkeleton SubwayPet;
    [Serialized] public SpineSkeleton StackedBurgerPet;
    [Serialized] public SpineSkeleton HotDogPet;
    [Serialized] public SpineSkeleton PizzaPet;
    [Serialized] public SpineSkeleton FoodFacePet;
    [Serialized] public SpineSkeleton DogPet;
    [Serialized] public SpineSkeleton LizardPet;
    [Serialized] public SpineSkeleton MolePet;
    [Serialized] public SpineSkeleton SpiderPet;
    [Serialized] public SpineSkeleton FruitJellySlimePet;
    [Serialized] public SpineSkeleton PizzaMonsterPet;
    [Serialized] public SpineSkeleton DonutGoatPet;
    [Serialized] public SpineSkeleton SherbertLumpPet;

    public UI.NineSlice FrameSlice = new UI.NineSlice() { slice = new Vector4(8, 8, 8, 8), sliceScale = 1f };
    public UI.NineSlice ButtonSlice = new UI.NineSlice() { slice = new Vector4(30, 30, 30, 30), sliceScale = 1f };
    public UI.NineSlice TopBarSlice = new UI.NineSlice() { slice = new Vector4(40, 40, 40, 40), sliceScale = 1f };

    public UI.TextSettings NoTextSettings = new UI.TextSettings() { size = 0, color = Vector4.Zero };

    public Vector4 RedText    = new Vector4(255.0f/255.0f, 109.0f/255.0f, 119.0f/255.0f, 1.0f);
    public Vector4 GreenText  = new Vector4( 80.0f/255.0f, 205.0f/255.0f, 109.0f/255.0f, 1.0f);
    public Vector4 BlueText   = new Vector4( 72.0f/255.0f, 235.0f/255.0f, 251.0f/255.0f, 1.0f);
    public Vector4 YellowText = new Vector4(255.0f/255.0f, 230.0f/255.0f, 100.0f/255.0f, 1.0f);

    public Vector4 BlueBg = new Vector4(101.0f/255.0f, 241.0f/255.0f, 250.0f/255.0f, 1.0f);

    public static References Instance;

    public override void Start()
    {
        Instance = this;
    }
}