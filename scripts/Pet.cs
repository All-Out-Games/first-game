using AO;

public class Pet : Component
{
    [AOIgnore] public static List<Pet> AllPets = new List<Pet>();

    [Serialized] public ulong OwnerId;
    [Serialized] public string DefinitionId;
    [Serialized] public string Name;
    [Serialized] public string PetId;

    public Vector2 Velocity;

    public bool Arrived;

    public PetData.PetDefinition Definition => PetData.Pets[DefinitionId];

    public override void Start()
    {
        // TODO spawn spine stuff, apply skins and what not
        AllPets.Add(this);

        Entity.GetComponent<Sprite_Renderer>().Sprite = Assets.GetAsset<Texture>(Definition.Sprite);
    }

    public override void Update()
    {
        var ownerEntity = Entity.FindByNetworkId(OwnerId);
        if (ownerEntity == null) {
            Entity.Destroy();
            return;
        }

        float agentRadius = 0.5f;
        var otherOwnerPets = AllPets.Where(p => p.OwnerId == OwnerId && p != this);
        var targetPosition = ownerEntity.Position + new Vector2(-ownerEntity.LocalScale.X, 0.5f);
        var distanceToTarget = (targetPosition - Entity.Position).Length;
        if (distanceToTarget < 0.5f)
        {
            Arrived = true;
        }
        else
        {
            foreach (var other in otherOwnerPets)
            {
                // if we are near another pet that has arrived, then we have arrived too
                if (other.Arrived && (other.Entity.Position - Entity.Position).Length < (agentRadius + agentRadius + 0.5f))
                {
                    Arrived = true;
                }
            }
        }

        Vector2 moveToTargetForce = Vector2.Zero;
        if (!Arrived)
        {
            var dirToTarget = (targetPosition - Entity.Position).Normalized;
            moveToTargetForce = dirToTarget;
        }

        Vector2 separationForce = Vector2.Zero;
        foreach (var other in otherOwnerPets)
        {
            float distance = (other.Entity.Position - Entity.Position).Length;
            float minDistance = (agentRadius + agentRadius);
            if (distance > minDistance)
            {
                continue;
            }
            var dirFromOther = (Entity.Position - other.Entity.Position);
            var divisor = 1.0f - distance / minDistance;
            if (distance == 0)
            {
                dirFromOther = new Vector2(1, 0);
            }
            else
            {
                dirFromOther /= distance;
            }
            separationForce += dirFromOther * divisor;
        }

        var totalForce = moveToTargetForce * 1.0f;
        totalForce += separationForce * 0.5f;
        totalForce *= (float)Math.Max(distanceToTarget, 1.0f);
        Velocity += totalForce * Time.DeltaTime * 15.0f;
        Velocity *= 0.85f;
        Entity.Position += Velocity * Time.DeltaTime;
    }

    public override void OnDestroy()
    {
        AllPets.Remove(this);
    }
}