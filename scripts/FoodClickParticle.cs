using AO;

public class FoodClickParticleSystem : System<FoodClickParticleSystem>
{
    public List<FoodClickParticle> ActiveParticles = new();

    public void SpawnParticle(Vector2 pos)
    {
        var particle = new FoodClickParticle();
        particle.Position = pos;

        var rng = new Random();
        var degrees = 90f + (rng.NextDouble() * 2 - 1) * 30;
        var radians = (Math.PI / 180f) * degrees;
        var dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        var force = AOMath.Lerp(0.65f, 1.0f, rng.NextFloat());
        particle.Velocity = dir * 5 * force;
        ActiveParticles.Add(particle);
    }

    public override void Update()
    {
        if (Network.IsServer) return;
        
        UI.PushLayerRelative(2);
        using var _2 = AllOut.Defer(UI.PopLayer);

        for (int i = ActiveParticles.Count-1; i >= 0; i--)
        {
            var particle = ActiveParticles[i];
            particle.Velocity.Y -= 15 * Time.DeltaTime;
            // particle.Velocity *= 0.9f;
            particle.Position += particle.Velocity * Time.DeltaTime;
            particle.Lifetime += Time.DeltaTime;
            var screenPos = Camera.WorldToScreen(particle.Position);
            float t = Ease.T(particle.Lifetime, 2.0f);
            var rect = new Rect(screenPos);
            var color = new Vector4(1, .25f, .25f, 1);
            color *= Ease.OutQuart(1-t);
            UI.Text(rect, "1", new UI.TextSettings() {
                color = color,
                size = 32,
                horizontalAlignment = UI.TextSettings.HorizontalAlignment.Center,
                verticalAlignment = UI.TextSettings.VerticalAlignment.Center,
                outline = true,
                outlineThickness = 2,
            });
            ActiveParticles[i] = particle;
            if (particle.Lifetime >= 1)
            {
                ActiveParticles.UnorderedRemoveAt(i);
            }
        }
    }
}

public struct FoodClickParticle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float   Lifetime;
}