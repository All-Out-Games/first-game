using AO;

public class FooComponent : TestComponent
{
    [Serialized] public TestComponent TestComponent;
    [Serialized] public Entity NestedEntity;

    public override void Update()
    {
        double t = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        NestedEntity.X = (float)Math.Sin(t * 2 * Math.PI);

        Log.Info($"{Entity.Id} {Id} {TestComponent.Entity.Id} {TestComponent.Id}");
        Log.Info($"{Entity.Id} {Id} {OtherTestComponent1.Entity.Id} {OtherTestComponent1.Id}");
        Log.Info($"{Entity.Id} {Id} {FooComponent.Entity.Id} {FooComponent.Id}");
    }
}