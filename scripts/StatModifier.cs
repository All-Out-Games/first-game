using AO;

public enum StatModifierKind
{
    StomachSize,
    MouthSize,
    ChewSpeed,
    CashMultiplier,
    MoveSpeed,
}

public class StatModifier
{
    public StatModifierKind Kind;
    public float            MultiplyValue;

    // temporary buffs
    public int Id;
    public float TimeLeft;
}
