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

    public void DeletePet(string id)
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
        }

        OwnedPets.Remove(pet);

        if (Network.IsServer)
        {
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
    public static Dictionary<string, PetDefinition> Pets = new Dictionary<string, PetDefinition>()
    {
        { "pet0",  new () { Id = "pet0",  Name = "Jimbo",     Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  1.03f } } } },
        { "pet1",  new () { Id = "pet1",  Name = "Jimbo +1",  Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  1.08f } } } },
        { "pet2",  new () { Id = "pet2",  Name = "Jimbo +2",  Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  1.10f } } } },
        { "pet3",  new () { Id = "pet3",  Name = "Jimbo +3",  Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  1.30f } } } },

        { "pet4",  new () { Id = "pet4",  Name = "Jimbo +4",  Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  1.07f } } } },
        { "pet5",  new () { Id = "pet5",  Name = "Jimbo +5",  Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  1.14f } } } },
        { "pet6",  new () { Id = "pet6",  Name = "Jimbo +6",  Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  1.20f } } } },
        { "pet7",  new () { Id = "pet7",  Name = "Jimbo +7",  Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  1.40f } } } },

        { "pet8",  new () { Id = "pet8",  Name = "Jimbo +8",  Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  1.18f } } } },
        { "pet9",  new () { Id = "pet9",  Name = "Jimbo +9",  Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  1.24f } } } },
        { "pet10", new () { Id = "pet10", Name = "Jimbo +10", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  1.44f } } } },
        { "pet11", new () { Id = "pet11", Name = "Jimbo +11", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  1.89f } } } },

        { "pet12", new () { Id = "pet12", Name = "Jimbo +12", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  1.26f } } } },
        { "pet13", new () { Id = "pet13", Name = "Jimbo +13", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  1.60f } } } },
        { "pet14", new () { Id = "pet14", Name = "Jimbo +14", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  1.96f } } } },
        { "pet15", new () { Id = "pet15", Name = "Jimbo +15", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  2.50f } } } },

        { "pet16", new () { Id = "pet16", Name = "Jimbo +16", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  1.35f } } } },
        { "pet17", new () { Id = "pet17", Name = "Jimbo +17", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  1.38f } } } },
        { "pet18", new () { Id = "pet18", Name = "Jimbo +18", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  1.67f } } } },
        { "pet19", new () { Id = "pet19", Name = "Jimbo +19", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  2.10f } } } },

        { "pet20", new () { Id = "pet20", Name = "Jimbo +20", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  1.84f } } } },
        { "pet21", new () { Id = "pet21", Name = "Jimbo +21", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  2.18f } } } },
        { "pet22", new () { Id = "pet22", Name = "Jimbo +22", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  2.60f } } } },
        { "pet23", new () { Id = "pet23", Name = "Jimbo +23", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  3.30f } } } },

        { "pet24", new () { Id = "pet24", Name = "Jimbo +24", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  1.90f } } } },
        { "pet25", new () { Id = "pet25", Name = "Jimbo +25", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  2.22f } } } },
        { "pet26", new () { Id = "pet26", Name = "Jimbo +26", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  2.73f } } } },
        { "pet27", new () { Id = "pet27", Name = "Jimbo +27", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  3.45f } } } },

        { "pet28", new () { Id = "pet28", Name = "Jimbo +28", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  2.28f } } } },
        { "pet29", new () { Id = "pet29", Name = "Jimbo +29", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  2.80f } } } },
        { "pet30", new () { Id = "pet30", Name = "Jimbo +30", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  3.52f } } } },
        { "pet31", new () { Id = "pet31", Name = "Jimbo +31", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  4.00f } } } },

        { "pet32", new () { Id = "pet32", Name = "Jimbo +32", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  2.60f } } } },
        { "pet33", new () { Id = "pet33", Name = "Jimbo +33", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  2.90f } } } },
        { "pet34", new () { Id = "pet34", Name = "Jimbo +34", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  3.77f } } } },
        { "pet35", new () { Id = "pet35", Name = "Jimbo +35", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  5.53f } } } },

        { "pet36", new () { Id = "pet36", Name = "Jimbo +36", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  3.01f } } } },
        { "pet37", new () { Id = "pet37", Name = "Jimbo +37", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  3.84f } } } },
        { "pet38", new () { Id = "pet38", Name = "Jimbo +38", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  5.66f } } } },
        { "pet39", new () { Id = "pet39", Name = "Jimbo +39", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  6.30f } } } },

        { "pet40", new () { Id = "pet40", Name = "Jimbo +40", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  3.97f } } } },
        { "pet41", new () { Id = "pet41", Name = "Jimbo +41", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  5.74f } } } },
        { "pet42", new () { Id = "pet42", Name = "Jimbo +42", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  6.52f } } } },
        { "pet43", new () { Id = "pet43", Name = "Jimbo +43", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  7.81f } } } },

        { "pet44", new () { Id = "pet44", Name = "Jimbo +44", Rarity = Rarity.Common,    Description = "A very cool pet.", Sprite = "pets/cow.png",           StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue =  5.96f } } } },
        { "pet45", new () { Id = "pet45", Name = "Jimbo +45", Rarity = Rarity.Uncommon,  Description = "A very cool pet.", Sprite = "pets/dire_wolf.png",     StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.ChewSpeed,   MultiplyValue =  6.85f } } } },
        { "pet46", new () { Id = "pet46", Name = "Jimbo +46", Rarity = Rarity.Epic,      Description = "A very cool pet.", Sprite = "pets/America_Eagle.png", StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue =  8.10f } } } },
        { "pet47", new () { Id = "pet47", Name = "Jimbo +47", Rarity = Rarity.Legendary, Description = "A very cool pet.", Sprite = "pets/Shady Dragon.png",  StatModifiers = new() { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue = 11.00f } } } },
    };

    public static Dictionary<string, EggDefinition> Eggs = new Dictionary<string, EggDefinition>()
    {
        // world 1
        { "egg1a", new () { Id = "egg1a", Name = "Egg 1A", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet0", Weight = 65 },
            new () { Id = "pet1", Weight = 25 },
            new () { Id = "pet2", Weight = 14 },
            new () { Id = "pet3", Weight = 1 },
        } } },

        { "egg1b", new () { Id = "egg1b", Name = "Egg 1B", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet4", Weight = 35 },
            new () { Id = "pet5", Weight = 40 },
            new () { Id = "pet6", Weight = 20 },
            new () { Id = "pet7", Weight = 5 },
        } } },

        { "egg1c", new () { Id = "egg1c", Name = "Egg 1C", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet8", Weight = 50 },
            new () { Id = "pet9", Weight = 35 },
            new () { Id = "pet10", Weight = 12 },
            new () { Id = "pet11", Weight = 3 },
        } } },

        { "egg1d", new () { Id = "egg1d", Name = "Egg 1D", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet12", Weight = 58 },
            new () { Id = "pet13", Weight = 25 },
            new () { Id = "pet14", Weight = 15 },
            new () { Id = "pet15", Weight = 2 },
        } } },

        // world 2
        { "egg2a", new () { Id = "egg2a", Name = "Egg 2A", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet16", Weight = 30 },
            new () { Id = "pet17", Weight = 30 },
            new () { Id = "pet18", Weight = 25 },
            new () { Id = "pet19", Weight = 15 },
        } } },

        { "egg2b", new () { Id = "egg2b", Name = "Egg 2B", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet20", Weight = 50 },
            new () { Id = "pet21", Weight = 25 },
            new () { Id = "pet22", Weight = 15 },
            new () { Id = "pet23", Weight = 10 },
        } } },

        { "egg2c", new () { Id = "egg2c", Name = "Egg 2C", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet24", Weight = 25 },
            new () { Id = "pet25", Weight = 40 },
            new () { Id = "pet26", Weight = 20 },
            new () { Id = "pet27", Weight = 15 },
        } } },

        { "egg2d", new () { Id = "egg2d", Name = "Egg 2D", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet28", Weight = 55 },
            new () { Id = "pet29", Weight = 25 },
            new () { Id = "pet30", Weight = 17 },
            new () { Id = "pet31", Weight = 3 },
        } } },

        // world 2
        { "egg3a", new () { Id = "egg3a", Name = "Egg 3A", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet32", Weight = 50 },
            new () { Id = "pet33", Weight = 30 },
            new () { Id = "pet34", Weight = 15 },
            new () { Id = "pet35", Weight = 5 },
        } } },

        { "egg3b", new () { Id = "egg3b", Name = "Egg 3B", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet36", Weight = 40 },
            new () { Id = "pet37", Weight = 40 },
            new () { Id = "pet38", Weight = 12 },
            new () { Id = "pet39", Weight = 8 },
        } } },

        { "egg3c", new () { Id = "egg3c", Name = "Egg 3C", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet40", Weight = 45 },
            new () { Id = "pet41", Weight = 35 },
            new () { Id = "pet42", Weight = 16 },
            new () { Id = "pet43", Weight = 4 },
        } } },

        { "egg3d", new () { Id = "egg3d", Name = "Egg 3D", PossiblePets = new List<WeightedPet>() {
            new () { Id = "pet44", Weight = 55 },
            new () { Id = "pet45", Weight = 30 },
            new () { Id = "pet46", Weight = 14 },
            new () { Id = "pet47", Weight = 1 },
        } } },
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
        StomachSize,
        MouthSize,
        ChewSpeed,
    }

    public class StatModifier
    {
        public StatModifierKind Kind;
        public float            MultiplyValue;
        public int              AddValue;
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    public class PetDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public string Sprite;
        public Rarity Rarity;
        public List<StatModifier> StatModifiers;
    }
}
