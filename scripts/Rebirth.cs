using AO;

public class Rebirth : System<Rebirth>
{
    public static List<RebirthData> RebirthData = new List<RebirthData>()
    {
        new() { RankName = "Noob",              CashMultiplier = 1.0, TrophiesCost = 10 },
        new() { RankName = "Rookie",            CashMultiplier = 1.2, TrophiesCost = 25 },
        new() { RankName = "Beginner",          CashMultiplier = 1.4, TrophiesCost = 50 },
        new() { RankName = "Novice",            CashMultiplier = 1.6, TrophiesCost = 100 },
        new() { RankName = "Apprentice",        CashMultiplier = 1.8, TrophiesCost = 250 },
        new() { RankName = "Intermediate",      CashMultiplier = 2.0, TrophiesCost = 500 },
        new() { RankName = "Standard",          CashMultiplier = 2.2, TrophiesCost = 1000 },
        new() { RankName = "Adept",             CashMultiplier = 2.4, TrophiesCost = 2500 },
        new() { RankName = "Efficient",         CashMultiplier = 2.6, TrophiesCost = 5000 },
        new() { RankName = "Experienced",       CashMultiplier = 2.8, TrophiesCost = 10000 },
        new() { RankName = "Advanced",          CashMultiplier = 3.0, TrophiesCost = 25000 },
        new() { RankName = "Profficient",       CashMultiplier = 3.2, TrophiesCost = 50000 },
        new() { RankName = "Skilled",           CashMultiplier = 3.4, TrophiesCost = 100000 },
        new() { RankName = "Semi-Pro",          CashMultiplier = 3.6, TrophiesCost = 250000 },
        new() { RankName = "Veteran",           CashMultiplier = 3.8, TrophiesCost = 500000 },
        new() { RankName = "Expert",            CashMultiplier = 4.0, TrophiesCost = 1000000 },
        new() { RankName = "Super Skilled",     CashMultiplier = 4.2, TrophiesCost = 2500000 },
        new() { RankName = "Professional",      CashMultiplier = 4.4, TrophiesCost = 5000000 },
        new() { RankName = "Elite",             CashMultiplier = 4.6, TrophiesCost = 10000000 },
        new() { RankName = "Specialist",        CashMultiplier = 4.8, TrophiesCost = 25000000 },
        new() { RankName = "Super Elite",       CashMultiplier = 5.0, TrophiesCost = 50000000 },
        new() { RankName = "Ultra Skilled",     CashMultiplier = 5.2, TrophiesCost = 100000000 },
        new() { RankName = "World Class",       CashMultiplier = 5.4, TrophiesCost = 250000000 },
        new() { RankName = "Ultra Elite",       CashMultiplier = 5.6, TrophiesCost = 500000000 },
        new() { RankName = "Semi-Master",       CashMultiplier = 5.8, TrophiesCost = 1000000000 },
        new() { RankName = "Prodigy",           CashMultiplier = 6.0, TrophiesCost = 2500000000 },
        new() { RankName = "Elite Specialist",  CashMultiplier = 6.2, TrophiesCost = 5000000000 },
        new() { RankName = "Master",            CashMultiplier = 6.4, TrophiesCost = 10000000000 },
        new() { RankName = "One of the Greats", CashMultiplier = 6.6, TrophiesCost = 25000000000 },
        new() { RankName = "Grandmaster",       CashMultiplier = 6.8, TrophiesCost = 50000000000 },
        new() { RankName = "Legend",            CashMultiplier = 7.0, TrophiesCost = 100000000000 },
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