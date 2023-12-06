using System.Runtime.Serialization;
using AO;
using TinyJson;

public partial class PetManager : Component
{
    public FatPlayer Player;

    public class OwnedPet
    {
        public string Id;
        public string Name;
        public bool Equipped;
        public string DefinitionId;

        public PetData.PetDefinition GetDefinition()
        { 
            if (!PetData.Pets.TryGetValue(DefinitionId, out var def)) {
                return null;
            }

            return def;
        }
    }

    [AOIgnore] public List<OwnedPet> OwnedPets = new List<OwnedPet>();

    public override void Start()
    {
    }

    public void LoadPetData(string data)
    {
        Log.Info("Loading pet data: " + data);
        OwnedPets = JSONParser.FromJson<List<OwnedPet>>(data);

        if (Network.IsServer) {
            SpawnPetsAsNeeded();
        }
    }

    public void AddPet(string petId, string petDefinitionId)
    {
        // TODO show a egg opening anim
        var newPet = new OwnedPet()
        {
            Id = petId,
            DefinitionId = petDefinitionId,
            Name = PetData.Pets[petDefinitionId].Name,
            Equipped = false,
        };
        OwnedPets.Add(newPet);

        if (Network.IsServer) {
            Save.SetString(Player, "AllPets", JSONWriter.ToJson(OwnedPets));
        }
    }

    public void EquipPet(string id)
    {
        var pet = OwnedPets.Find(p => p.Id == id);
        if (pet == null) 
        {
            Log.Error($"Could not find pet with id {id}");
            return;
        }

        var equipCount = OwnedPets.Count(p => p.Equipped);
        pet.Equipped = true;


        if (Network.IsServer) {
            SpawnPetsAsNeeded();
            Save.SetString(Player, "AllPets", JSONWriter.ToJson(OwnedPets));
        }
    }

    public void UnequipPet(string id)
    {
        var pet = OwnedPets.Find(p => p.Id == id);
        if (pet == null) 
        {
            Log.Error($"Could not find equipped pet");
            return;
        }

        pet.Equipped = false;

        if (Network.IsServer) 
        {
            foreach (var p in Pet.AllPets.ToList()) 
            {
                if (p.PetId == pet.Id) 
                {
                    Network.Despawn(p.Entity);
                    Pet.AllPets.Remove(p);
                    p.Entity.Destroy();
                    break;
                }
            }
            Save.SetString(Player, "AllPets", JSONWriter.ToJson(OwnedPets));
        }
    }

    public void SpawnPetsAsNeeded()
    {
        foreach(var pet in OwnedPets)
        {
            if (!pet.Equipped) continue;
            if (Pet.AllPets.Any(p => p.OwnerId == Player.Entity.NetworkId && p.PetId == pet.Id)) continue;

            var petEntity = Entity.Instantiate(References.Instance.PetPrefab);
            var p = petEntity.GetComponent<Pet>();
            p.OwnerId = Player.Entity.NetworkId;
            p.DefinitionId = pet.DefinitionId;
            p.Name = pet.Name;
            p.PetId = pet.Id;
            Network.Spawn(petEntity);
        }
    }
}

public static class PetData
{
    [AOIgnore] public static Dictionary<string, PetDefinition> Pets = new Dictionary<string, PetDefinition>() 
    {
        { "pet0", new () { Id = "pet0", Name = "Jimbo",   Description = "Jimbo is a very cool pet.",
            StatModifiers = new(){new StatModifier(){ Kind = StatModifierKind.ChewSpeedMultiply, MultiplyValue = 1.1f } } } },

        { "pet1", new () { Id = "pet1", Name = "Jimbo 1", Description = "Jimbo 1 is a very cool pet.",
            StatModifiers = new(){new StatModifier(){ Kind = StatModifierKind.ChewSpeedMultiply, MultiplyValue = 1.2f } } } },

        { "pet2", new () { Id = "pet2", Name = "Jimbo 2", Description = "Jimbo 2 is a very cool pet.",
            StatModifiers = new(){new StatModifier(){ Kind = StatModifierKind.ChewSpeedMultiply, MultiplyValue = 1.3f } } } },
    };

    [AOIgnore] public static Dictionary<string, EggDefinition> Eggs = new Dictionary<string, EggDefinition>() 
    {
        { "egg0", new () { Id = "edd0", Name = "Egg", PossiblePets = new List<WeightedPet>() { new () { Id = "pet0", Weight = 1 }, new () { Id = "pet1", Weight = 1 }, new () { Id = "pet2", Weight = 1 } } } },
    };

    public class EggDefinition
    {
        public string Id;
        public string Name;
        
        public List<WeightedPet> PossiblePets;
    }

    public class WeightedPet
    {
        public string Id;
        public int Weight;
    }

    public enum StatModifierKind
    {
        MaxFoodMultiply,
        MouthSizeMultiply,
        ChewSpeedMultiply,

        MaxFoodAdd,
        MouthSizeAdd,
        ChewSpeedAdd,
    }

    public class StatModifier
    {
        public StatModifierKind Kind;
        public float            MultiplyValue;
        public int              AddValue;
    }

    public class PetDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public List<StatModifier> StatModifiers;
    }
}