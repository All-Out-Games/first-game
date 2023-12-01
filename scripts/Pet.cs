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

        var targetPosition = ownerEntity.Position + new Vector2(-ownerEntity.LocalScale.X, 0.5f);
        // Move towards
        var distance = (Entity.Position - targetPosition).Length;
        if (distance > 0.1f) {
            var direction = (targetPosition - Entity.Position).Normalized;
            var speed = 5f;
            
            Entity.Position += direction * Time.DeltaTime * speed;
        }
    }
}