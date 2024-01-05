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
    [Serialized] public Texture BossBarBg;

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

    [Serialized] public SpineSkeletonAsset IceCreamConePet;
    [Serialized] public SpineSkeletonAsset BurgerPet;
    [Serialized] public SpineSkeletonAsset DrumstickPet;
    [Serialized] public SpineSkeletonAsset CarrotPet;
    [Serialized] public SpineSkeletonAsset PenguinPet;
    [Serialized] public SpineSkeletonAsset DiamondPet;
    [Serialized] public SpineSkeletonAsset FriesPet;
    [Serialized] public SpineSkeletonAsset SubwayPet;
    [Serialized] public SpineSkeletonAsset SqueezySaucePet;
    [Serialized] public SpineSkeletonAsset HotDogPet;
    [Serialized] public SpineSkeletonAsset PizzaPet;
    [Serialized] public SpineSkeletonAsset FoodFacePet;
    [Serialized] public SpineSkeletonAsset DogPet;
    [Serialized] public SpineSkeletonAsset LizardPet;
    [Serialized] public SpineSkeletonAsset MolePet;
    [Serialized] public SpineSkeletonAsset SpiderPet;
    [Serialized] public SpineSkeletonAsset FruitJellySlimePet;
    [Serialized] public SpineSkeletonAsset PizzaMonsterPet;
    [Serialized] public SpineSkeletonAsset DonutGoatPet;
    [Serialized] public SpineSkeletonAsset SherbertLumpPet;
    [Serialized] public SpineSkeletonAsset EggOpenAnimSkeleton;

    [Serialized] public Texture IconCarrotPet;
    [Serialized] public Texture IconDrumstickPet;
    [Serialized] public Texture IconFriesPet;
    [Serialized] public Texture IconIceCreamConePet;
    [Serialized] public Texture IconSubwayPet;
    [Serialized] public Texture IconSqueezySaucePet;
    [Serialized] public Texture IconPenguinPet;
    [Serialized] public Texture IconDiamondPet;
    [Serialized] public Texture IconFoodFacePet;
    [Serialized] public Texture IconHotDogPet;
    [Serialized] public Texture IconBurgerPet;
    [Serialized] public Texture IconPizzaPet;
    [Serialized] public Texture IconDogPet;
    [Serialized] public Texture IconMolePet;
    [Serialized] public Texture IconLizardPet;
    [Serialized] public Texture IconSpiderPet;
    [Serialized] public Texture IconDonutGoatPet;
    [Serialized] public Texture IconSherbertLumpPet;
    [Serialized] public Texture IconFruitJellySlimePet;
    [Serialized] public Texture IconPizzaMonsterPet;

    public UI.NineSlice DarkFrameSlice  = new UI.NineSlice() { slice = new Vector4(8, 8, 8, 8),     sliceScale = 1f };
    public UI.NineSlice WhiteFrameSlice = new UI.NineSlice() { slice = new Vector4(45, 45, 45, 45), sliceScale = 0.5f };
    public UI.NineSlice ButtonSlice     = new UI.NineSlice() { slice = new Vector4(30, 30, 30, 30), sliceScale = 1f };
    public UI.NineSlice TopBarSlice     = new UI.NineSlice() { slice = new Vector4(34, 34, 34, 34), sliceScale = 1f };
    public UI.NineSlice BossBarSlice    = new UI.NineSlice() { slice = new Vector4(34, 34, 34, 34), sliceScale = 1f };

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