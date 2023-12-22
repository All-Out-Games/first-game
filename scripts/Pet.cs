using AO;

public class Pet : Component
{
    [AOIgnore] public static List<Pet> AllPets = new List<Pet>();

    [Serialized] public ulong OwnerId;
    [Serialized] public string DefinitionId;
    [Serialized] public string Name;
    [Serialized] public string PetId;

    public Spine_Animator SpineAnimator;

    public Vector2 Velocity;

    public bool Arrived;

    public PetData.PetDefinition Definition => PetData.Pets[DefinitionId];

    public override void Start()
    {
        // TODO spawn spine stuff, apply skins and what not
        AllPets.Add(this);

        SpineAnimator = Entity.GetComponent<Spine_Animator>();
        SpineAnimator.Skeleton = Definition.Spine;
        SpineAnimator.SetSkin(Definition.Skin);
        SpineAnimator.SetAnimation("idle", true);

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
                if (other.Arrived && (other.Entity.Position - Entity.Position).Length < (agentRadius + agentRadius + 1.0f))
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
        if (totalForce.X < -0.25f) {
            Entity.LocalScaleX = -1;
        }
        else if (totalForce.X > 0.25f) {
            Entity.LocalScaleX = 1;
        }
        Velocity += totalForce * Time.DeltaTime * 15.0f;
        Velocity *= 0.85f;
        Entity.Position += Velocity * Time.DeltaTime;
        
        if (Velocity.Length > 0.1f && !isRunning)
        {
            isRunning = true;
            SpineAnimator.SetAnimation("run", true);
        }
        else if (Velocity.Length < 0.1f && isRunning)
        {
            isRunning = false;
            SpineAnimator.SetAnimation("idle", true);
        }
    }

    bool isRunning = false;

    public override void OnDestroy()
    {
        AllPets.Remove(this);
    }
}