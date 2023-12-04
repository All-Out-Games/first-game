using AO;

public class Pet : Component
{
    [AOIgnore] public static List<Pet> AllPets = new List<Pet>();

    [Serialized] public ulong OwnerId;
    [Serialized] public string DefinitionId;
    [Serialized] public string Name;
    [Serialized] public string PetId;

    public PetData.PetDefinition Definition => PetData.Pets[DefinitionId];

    public override void Start()
    {
        // TODO spawn spine stuff, apply skins and what not
        AllPets.Add(this);
    }

    public override void Update()
    {
        var ownerEntity = Entity.FindByNetworkId(OwnerId);
        if (ownerEntity == null) {
            Entity.Destroy();
            return;
        }

        var totalAccel = Vector2.Zero;

        var otherOwnerPets = AllPets.Where(p => p.OwnerId == OwnerId && p != this);
        foreach (var other in otherOwnerPets) {
            var vectorToPet = other.Entity.Position - Entity.Position;
            if (other.Entity.Position == Entity.Position) {
                vectorToPet = new Vector2(1, 0);
            }
            var dist = vectorToPet.Length;
            if (dist <= 1f) {       
                var modifier = 1 - (dist / 1f) + 1;
                totalAccel -= vectorToPet.Normalized * modifier;
            }
        }

        var targetPosition = ownerEntity.Position + new Vector2(-ownerEntity.LocalScale.X, 0.5f);
        // Move towards
        var distance = (Entity.Position - targetPosition).Length;
        if (distance > 0.1f) {
            var direction = (targetPosition - Entity.Position).Normalized;    
            totalAccel += direction;
        }

        var speed = 5f;
        Entity.Position += totalAccel * Time.DeltaTime * speed;
    }

    public override void OnDestroy()
    {
        AllPets.Remove(this);
    }
}