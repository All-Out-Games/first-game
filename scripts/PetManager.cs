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

    public List<OwnedPet> OwnedPets = new List<OwnedPet>();
    
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

    public void DeleteAllPets()
    {
        foreach (var pet in OwnedPets)
        {
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
        }
        OwnedPets.Clear();
        if (Network.IsServer)
        {
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
        { "Carrot",                 new () { Id = "Carrot",                Name = "Carrot",               Rarity = Rarity.Common,          Description = "A very cool pet.", Spine = References.Instance.CarrotPet,             Skin = "carrot",         RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue = 1.03f } } } },
        { "Chicken Drumstick",      new () { Id = "Chicken Drumstick",     Name = "Chicken Drumstick",    Rarity = Rarity.Common,          Description = "A very cool pet.", Spine = References.Instance.DrumstickPet,          Skin = "drumstick",      RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue = 1.03f } } } },
        { "Fries",                  new () { Id = "Fries",                 Name = "Fries",                Rarity = Rarity.Common,          Description = "A very cool pet.", Spine = References.Instance.FriesPet,              Skin = "fries",          RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.Money,       MultiplyValue = 1.03f } } } },
        { "Ice Cream",              new () { Id = "Ice Cream",             Name = "Ice Cream",            Rarity = Rarity.Common,          Description = "A very cool pet.", Spine = References.Instance.IceCreamConePet,       Skin = "ice_cream",      RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.ClickPower,  MultiplyValue = 1.08f } } } },
        { "Subway",                 new () { Id = "Subway",                Name = "Subway",               Rarity = Rarity.Uncommon,        Description = "A very cool pet.", Spine = References.Instance.SubwayPet,             Skin = "sandwich",       RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue = 1.10f } } } },
        { "Squeezy Sauce",          new () { Id = "Squeezy Sauce",         Name = "Squeezy Sauce",        Rarity = Rarity.Uncommon,        Description = "A very cool pet.", Spine = References.Instance.StackedBurgerPet,      Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue = 1.14f } } } },
        { "Pingu",                  new () { Id = "Pingu",                 Name = "Pingu",                Rarity = Rarity.Uncommon,        Description = "A very cool pet.", Spine = References.Instance.PenguinPet,            Skin = "penguin",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.ClickPower,  MultiplyValue = 1.20f } } } },
        { "Diamond",                new () { Id = "Diamond",               Name = "Diamond",              Rarity = Rarity.Uncommon,        Description = "A very cool pet.", Spine = References.Instance.DiamondPet,            Skin = "diamond",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.Money,       MultiplyValue = 1.20f } } } },
        { "Food Face",              new () { Id = "Food Face",             Name = "Food Face",            Rarity = Rarity.Rare,            Description = "A very cool pet.", Spine = References.Instance.FoodFacePet,           Skin = "food_face",      RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue = 1.25f } } } },
        { "Sausage",                new () { Id = "Sausage",               Name = "Sausage",              Rarity = Rarity.Rare,            Description = "A very cool pet.", Spine = References.Instance.HotDogPet,             Skin = "hotdog",         RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue = 1.35f } } } },
        { "Burger",                 new () { Id = "Burger",                Name = "Burger",               Rarity = Rarity.Rare,            Description = "A very cool pet.", Spine = References.Instance.BurgerPet,             Skin = "burger",         RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.Money,       MultiplyValue = 1.60f } } } },
        { "Pizza Slice",            new () { Id = "Pizza Slice",           Name = "Pizza Slice",          Rarity = Rarity.Rare,            Description = "A very cool pet.", Spine = References.Instance.PizzaPet,              Skin = "pizza",          RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.ClickPower,  MultiplyValue = 1.60f } } } },
        { "Golden Dog",             new () { Id = "Golden Dog",            Name = "Golden Dog",           Rarity = Rarity.Epic,            Description = "A very cool pet.", Spine = References.Instance.DogPet,                Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue = 1.25f } } } },
        { "Brown Mole",             new () { Id = "Brown Mole",            Name = "Brown Mole",           Rarity = Rarity.Epic,            Description = "A very cool pet.", Spine = References.Instance.MolePet,               Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.ClickPower,  MultiplyValue = 1.50f } } } },
        { "Green Lizard",           new () { Id = "Green Lizard",          Name = "Green Lizard",         Rarity = Rarity.Epic,            Description = "A very cool pet.", Spine = References.Instance.LizardPet,             Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.Money,       MultiplyValue = 1.75f } } } },
        { "Black Widow Spider",     new () { Id = "Black Widow Spider",    Name = "Black Widow Spider",   Rarity = Rarity.Epic,            Description = "A very cool pet.", Spine = References.Instance.SpiderPet,             Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue = 1.90f } } } },
        { "Donut Goat",             new () { Id = "Donut Goat",            Name = "Donut Goat",           Rarity = Rarity.Legendary,       Description = "A very cool pet.", Spine = References.Instance.DonutGoatPet,          Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.ClickPower,  MultiplyValue = 1.60f } } } },
        { "Sherbert Lump",          new () { Id = "Sherbert Lump",         Name = "Sherbert Lump",        Rarity = Rarity.Legendary,       Description = "A very cool pet.", Spine = References.Instance.SherbertLumpPet,       Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.StomachSize, MultiplyValue = 2.15f } } } },
        { "Slime",                  new () { Id = "Slime",                 Name = "Slime",                Rarity = Rarity.Legendary,       Description = "A very cool pet.", Spine = References.Instance.FruitJellySlimePet,    Skin = "default",        RunAnimName = "walk",    StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.MouthSize,   MultiplyValue = 2.35f } } } },
        { "Pizza Monster",          new () { Id = "Pizza Monster",         Name = "Pizza Monster",        Rarity = Rarity.Legendary,       Description = "A very cool pet.", Spine = References.Instance.PizzaMonsterPet,       Skin = "default",        RunAnimName = "run",     StatModifiers = new () { new StatModifier() { Kind = StatModifierKind.Money,       MultiplyValue = 2.50f } } } },
    };

    public static Dictionary<string, EggDefinition> Eggs = new Dictionary<string, EggDefinition>()
    {
        { "Fruit Egg", new () { PossiblePets = new List<WeightedPet>() {
            new () { Id = "Carrot",        Weight = 20 },
            new () { Id = "Chicken",       Weight = 20 },
            new () { Id = "Fries",         Weight = 20 },
            new () { Id = "Subway",        Weight = 15 },
            new () { Id = "Golden Dog",    Weight = 1 },
        } } },

        { "Salad Egg", new () { PossiblePets = new List<WeightedPet>() {
            new () { Id = "Ice Cream",     Weight = 15 },
            new () { Id = "Squeezy Sauce", Weight = 20 },
            new () { Id = "Food Face",     Weight = 12 },
            new () { Id = "Brown Mole",    Weight = 8 },
            new () { Id = "Donut Goat",    Weight = 1 },
        } } },

        { "Sundae Egg", new () { PossiblePets = new List<WeightedPet>() {
            new () { Id = "Pingu",         Weight = 15 },
            new () { Id = "Diamond",       Weight = 15 },
            new () { Id = "Sausage",       Weight = 12 },
            new () { Id = "Green Lizard",  Weight = 10 },
            new () { Id = "Sherbert Lump", Weight = 5 },
        } } },

        { "Mac and Cheese Egg", new () { PossiblePets = new List<WeightedPet>() {
            new () { Id = "Burger",        Weight = 10 },
            new () { Id = "Pizza Slice",   Weight = 10 },
            new () { Id = "Black Widow",   Weight = 15 },
            new () { Id = "Slime",         Weight = 7 },
            new () { Id = "Pizza Monster", Weight = 5 },
        } } },

        // { "Sushi Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

        // { "Pepperoni Pizza Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

        // { "Lasagna Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

        // { "Burger Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

        // { "Mint Chocolate Chip Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

        // { "Dripping Honeycomb Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

        // { "Chocolate Cookies Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

        // { "Rainbow Mega Swirl Egg", new () { PossiblePets = new List<WeightedPet>() {
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        //     new () { Id = "", Weight = 10 },
        // } } },

    };

    public class EggDefinition
    {
        public List<WeightedPet> PossiblePets;
    }

    public class WeightedPet
    {
        public string Id;
        public int Weight;
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
        public SpineSkeleton Spine;
        public string Skin;
        public Rarity Rarity;
        public string RunAnimName;
        public List<StatModifier> StatModifiers;
    }
}
