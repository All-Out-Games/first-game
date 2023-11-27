using AO;

public class References : Component
{
    [Serialized] public Texture CoinIcon;
    [Serialized] public Texture FoodIcon;
    [Serialized] public Texture MouthSizeIcon;
    [Serialized] public Texture ChewSpeedIcon;

    public static References Instance;

    public override void Start()
    {
        Instance = this;
    }
}