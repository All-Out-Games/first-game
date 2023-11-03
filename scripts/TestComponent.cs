using AO;

public class TestComponent : Component
{
    [Serialized] public float Speed;
    
    public override void Update()
    {
        double t = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        Vector2 position = Entity.Position;
        position.X = (float)Math.Sin(t * 2 * Math.PI * Speed);
        position.Y = (float)Math.Cos(t * 2 * Math.PI * Speed);
        Entity.Position = position;
    }
}