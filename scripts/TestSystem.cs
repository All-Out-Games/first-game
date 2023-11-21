using AO;

public class TestSystem : System<TestSystem>
{
    public const float ToRadians = 0.0174533f;
    public const float ToDegrees = 57.2958f;

    public override void Update()
    {
        double t = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        double rotation = (t * 45 % 360);
        foreach (var entity in Scene.AllEntities)
        {
            // entity.Rotation = (float)rotation;
        }
    }
}
