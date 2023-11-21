using AO;

public class Thingerino
{
    [Serialized] public string NestedString;
    [Serialized] public string[] NestedStringArray;
}

public class TestComponent : Component
{
    public static bool DidCloneTest = false;

    [Serialized] public FooComponent FooComponent;
    [Serialized] public Entity CloneTest;
    [Serialized] public TestComponent OtherTestComponent1;
    [Serialized] public TestComponent OtherTestComponent2;
    [Serialized] public Sprite_Renderer SomeSpriteRenderer;
    [Serialized] public float Speed;
    [Serialized] public Thingerino Thingerino1;
    [Serialized] public Thingerino Thingerino2;
    [Serialized] public Thingerino Thingerino3;
    [Serialized] public float[] FloatArray1;
    [Serialized] public float[] FloatArray2;
    [Serialized] public float[] FloatArray3;
    [Serialized] public float Distance = 1;
    
    public override void Awake()
    {
        if (!DidCloneTest && CloneTest != null)
        {
            DidCloneTest = true;
            // Log.Info($"Doing clone test {Entity.Id}");
            Entity newEntity = Entity.Clone();
            // Log.Info($"New entity: {newEntity.Id}");
            TestComponent otherTest = newEntity.GetComponent<TestComponent>();
            // Log.Info($"otherTest: {otherTest}");
            // Log.Info($"otherTest entity id: {otherTest.Entity.Id}");
            // Log.Info($"otherTest.Entity == Entity: {otherTest.Entity == Entity}");
            // Log.Info($"otherTest.Entity.Id == Entity.Id: {otherTest.Entity.Id == Entity.Id}");
            newEntity.X += 1;
            newEntity.Y += 0.5f;
            Entity.X -= 1;

            Log.Info($"{Entity.Id}, {Id}, {OtherTestComponent1.Id}, {OtherTestComponent2.Id}, {OtherTestComponent1.Entity.Id}, {OtherTestComponent2.Entity.Id}");
            Log.Info($"{otherTest.Entity.Id}, {otherTest.Id}, {otherTest.OtherTestComponent1.Id}, {otherTest.OtherTestComponent2.Id}, {otherTest.OtherTestComponent1.Entity.Id}, {otherTest.OtherTestComponent2.Entity.Id}");

            Log.Info($"{FooComponent.Id}, {FooComponent.Entity.Id}, {FooComponent.TestComponent.Id}, {FooComponent.TestComponent.Entity.Id}");
            Log.Info($"{otherTest.FooComponent.Id}, {otherTest.FooComponent.Entity.Id}, {otherTest.FooComponent.TestComponent.Id}, {otherTest.FooComponent.TestComponent.Entity.Id}");

        //     var otherTest = newEntity.GetComponent<TestComponent>();
        //     Log.Info($"otherTest: {otherTest}");
        //     Log.Info($"otherTest == this: {otherTest == this}");
        //     Log.Info($"{otherTest.CloneTest == newEntity}");
        //     Log.Info($"{otherTest.CloneTest == Entity}");

        //     Log.Info("---- Should all be TRUE ----");
        //     Log.Info($"{otherTest.Thingerino1.NestedString == "some stringerino"}");
        //     Log.Info($"{otherTest.Thingerino1.NestedStringArray.Length == 0}");
        //     Log.Info($"{string.IsNullOrEmpty(otherTest.Thingerino2.NestedString)}");
        //     Log.Info($"{otherTest.Thingerino2.NestedStringArray.Length == 0}");
        //     Log.Info($"{string.IsNullOrEmpty(otherTest.Thingerino3.NestedString)}");
        //     Log.Info($"{otherTest.Thingerino3.NestedStringArray.Length == 2}");
        //     Log.Info($"{otherTest.Thingerino3.NestedStringArray[0] == "hello"}");
        //     Log.Info($"{otherTest.Thingerino3.NestedStringArray[1] == "world!! wowza neato."}");

        //     Log.Info($"{otherTest.FloatArray1.Length == 1}");
        //     Log.Info($"{otherTest.FloatArray1[0] == 12}");
        //     Log.Info($"{otherTest.FloatArray2.Length == 2}");
        //     Log.Info($"{otherTest.FloatArray2[0] == 12}");
        //     Log.Info($"{otherTest.FloatArray2[1] == 34}");

        //     Log.Info("---- Should all be FALSE ----");
        //     Log.Info($"{otherTest.Thingerino1.NestedString != "some stringerino"}");
        //     Log.Info($"{otherTest.Thingerino1.NestedStringArray.Length != 0}");
        //     Log.Info($"{!string.IsNullOrEmpty(otherTest.Thingerino2.NestedString)}");
        //     Log.Info($"{otherTest.Thingerino2.NestedStringArray.Length != 0}");
        //     Log.Info($"{!string.IsNullOrEmpty(otherTest.Thingerino3.NestedString)}");
        //     Log.Info($"{otherTest.Thingerino3.NestedStringArray.Length != 2}");
        //     Log.Info($"{otherTest.Thingerino3.NestedStringArray[0] != "hello"}");
        //     Log.Info($"{otherTest.Thingerino3.NestedStringArray[1] != "world!! wowza neato."}");

        //     Log.Info($"{otherTest.FloatArray1.Length != 1}");
        //     Log.Info($"{otherTest.FloatArray1[0] != 12}");
        //     Log.Info($"{otherTest.FloatArray2.Length != 2}");
        //     Log.Info($"{otherTest.FloatArray2[0] != 12}");
        //     Log.Info($"{otherTest.FloatArray2[1] != 34}");
        }
    }

    public override void Update()
    {
        // Log.Info($"SomeSpriteRenderer: '{SomeSpriteRenderer}'");
        if (SomeSpriteRenderer != null) {
            double t = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            SomeSpriteRenderer.DepthOffset = (float)Math.Sin(t * 2 * Math.PI);
            // Log.Info($"Other sprite is on entity '{SomeSpriteRenderer.Entity.Id}'. I am entity '{Entity.Id}'");
        }

        // if (Test1A != null)
        // {
        //     Log.Info($"Test1A: {Test1A}, Test1B: {Test1B}, Test1A == Test1B: {Test1A == Test1B}, Test1A.Id: {Test1A.Id}, Test1B.Id: {Test1B.Id}, Test1A.Id == Test1B.Id: {Test1A.Id == Test1B.Id}");
        //     Log.Info($"Test2A: {Test2A}, Test2B: {Test2B}, Test2A == Test2B: {Test2A == Test2B}, Test2A.Id: {Test2A.Id}, Test2B.Id: {Test2B.Id}, Test2A.Id == Test2B.Id: {Test2A.Id == Test2B.Id}");
        //     Log.Info($"Test1A: {Test1A}, Test2A: {Test2A}, Test1A == Test2A: {Test1A == Test2A}, Test1A.Id: {Test1A.Id}, Test2A.Id: {Test2A.Id}, Test1A.Id == Test2A.Id: {Test1A.Id == Test2A.Id}");
        // }

        // if (SomeNumbers != null && SomeNumbers.Length > 0)
        // {
        //     foreach (var num in SomeNumbers)
        //     {
        //         Log.Info($"Number: {num}");
        //     }
        // }
        // if (OtherEntities != null && OtherEntities.Length > 0)
        // {
        //     foreach (var other in OtherEntities)
        //     {
        //         Log.Info($"Other entity {other.Name} ({other.Id})");
        //     }
        // }
        // double t = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
        // Vector2 position = EntityToSpin.Position;
        // position.X = (float)Math.Sin(t * 2 * Math.PI * Speed) * Distance;
        // position.Y = (float)Math.Cos(t * 2 * Math.PI * Speed) * Distance;
        // EntityToSpin.Position = position;
        // EntityToSpin.Rotation = (float)((t * 45) % 360);

        // {
        //     Log.Info("------------------------------");

        //     foreach (var entity in Scene.EntityIterator())
        //     {
        //         uint idx = (uint)(entity.Id >> 32);
        //         uint gen = (uint)entity.Id;
        //         Log.Info($"entity: {idx}:{gen}" + ((entity.Id == Entity.Id) ? " (me)" : ""));
        //     }

        //     int cursor = -1;
        //     while (Scene.GetNextEntity(ref cursor, out Entity e))
        //     {
        //         uint idx = (uint)(e.Id >> 32);
        //         uint gen = (uint)e.Id;
        //         Log.Info($"cursor: {cursor}, entity: {idx}:{gen}" + ((e.Id == Entity.Id) ? " (me)" : ""));
        //     }
        // }
    }
}