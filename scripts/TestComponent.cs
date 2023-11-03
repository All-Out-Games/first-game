using AO;

public class TestComponent : Component
{
    [Serialized] public Entity EntityToSpin;
    [Serialized] public float Speed;
    [Serialized] public float Distance = 1;
    
    public override void Update()
    {
        double t = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        Vector2 position = EntityToSpin.Position;
        position.X = (float)Math.Sin(t * 2 * Math.PI * Speed) * Distance;
        position.Y = (float)Math.Cos(t * 2 * Math.PI * Speed) * Distance;
        EntityToSpin.Position = position;
        EntityToSpin.Rotation = (float)((t * 45) % 360);
    }
}