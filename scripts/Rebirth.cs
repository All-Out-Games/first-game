using AO;

public class Rebirth : System<Rebirth>
{
    public static List<RebirthData> RebirthData = new List<RebirthData>()
    {
        new() { RankName = "Noob",              CashMultiplier = 1.0, TrophiesCost = 10 },
        new() { RankName = "Rookie",            CashMultiplier = 1.1, TrophiesCost = 25 },
        new() { RankName = "Beginner",          CashMultiplier = 1.2, TrophiesCost = 50 },
        new() { RankName = "Novice",            CashMultiplier = 1.3, TrophiesCost = 100 },
        new() { RankName = "Apprentice",        CashMultiplier = 1.4, TrophiesCost = 250 },
        new() { RankName = "Intermediate",      CashMultiplier = 1.5, TrophiesCost = 500 },
        new() { RankName = "Standard",          CashMultiplier = 1.6, TrophiesCost = 1000 },
        new() { RankName = "Adept",             CashMultiplier = 1.7, TrophiesCost = 2500 },
        new() { RankName = "Efficient",         CashMultiplier = 1.8, TrophiesCost = 5000 },
        new() { RankName = "Experienced",       CashMultiplier = 2.9, TrophiesCost = 10000 },
        new() { RankName = "Advanced",          CashMultiplier = 2.0, TrophiesCost = 25000 },
        new() { RankName = "Profficient",       CashMultiplier = 2.1, TrophiesCost = 50000 },
        new() { RankName = "Skilled",           CashMultiplier = 2.2, TrophiesCost = 100000 },
        new() { RankName = "Semi-Pro",          CashMultiplier = 2.3, TrophiesCost = 250000 },
        new() { RankName = "Veteran",           CashMultiplier = 2.4, TrophiesCost = 500000 },
        new() { RankName = "Expert",            CashMultiplier = 2.5, TrophiesCost = 1000000 },
        new() { RankName = "Super Skilled",     CashMultiplier = 2.6, TrophiesCost = 2500000 },
        new() { RankName = "Professional",      CashMultiplier = 2.7, TrophiesCost = 5000000 },
        new() { RankName = "Elite",             CashMultiplier = 2.8, TrophiesCost = 10000000 },
        new() { RankName = "Specialist",        CashMultiplier = 3.9, TrophiesCost = 25000000 },
        new() { RankName = "Super Elite",       CashMultiplier = 3.0, TrophiesCost = 50000000 },
        new() { RankName = "Ultra Skilled",     CashMultiplier = 3.1, TrophiesCost = 100000000 },
        new() { RankName = "World Class",       CashMultiplier = 3.2, TrophiesCost = 250000000 },
        new() { RankName = "Ultra Elite",       CashMultiplier = 3.3, TrophiesCost = 500000000 },
        new() { RankName = "Semi-Master",       CashMultiplier = 3.4, TrophiesCost = 1000000000 },
        new() { RankName = "Prodigy",           CashMultiplier = 3.5, TrophiesCost = 2500000000 },
        new() { RankName = "Elite Specialist",  CashMultiplier = 3.6, TrophiesCost = 5000000000 },
        new() { RankName = "Master",            CashMultiplier = 3.7, TrophiesCost = 10000000000 },
        new() { RankName = "One of the Greats", CashMultiplier = 3.8, TrophiesCost = 25000000000 },
        new() { RankName = "Grandmaster",       CashMultiplier = 4.9, TrophiesCost = 50000000000 },
        new() { RankName = "Legend",            CashMultiplier = 4.0, TrophiesCost = 100000000000 },
    };

    public RebirthData GetRebirthData(int rebirth)
    {
        if (rebirth >= RebirthData.Count)
            rebirth = RebirthData.Count - 1;
        return RebirthData[rebirth];
    }
}

public class RebirthData
{
    public string RankName;
    public double CashMultiplier;
    public double TrophiesCost;
}