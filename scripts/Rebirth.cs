using AO;

public class Rebirth : System<Rebirth>
{
    [AOIgnore] public List<RebirthData> RebirthData = new List<RebirthData>()
    {
        new () { RankName = "Noob",         CashMultiplier = 1.0f, TrophiesCost = 0    },
        new () { RankName = "Rookie",       CashMultiplier = 1.1f, TrophiesCost = 100  },
        new () { RankName = "Beginner",     CashMultiplier = 1.1f, TrophiesCost = 100  },
        new () { RankName = "Intermediate", CashMultiplier = 1.2f, TrophiesCost = 200  },
        new () { RankName = "Advanced",     CashMultiplier = 1.3f, TrophiesCost = 300  },
        new () { RankName = "Expert",       CashMultiplier = 1.4f, TrophiesCost = 400  },
        new () { RankName = "Master",       CashMultiplier = 1.5f, TrophiesCost = 500  },
        new () { RankName = "Grandmaster",  CashMultiplier = 1.6f, TrophiesCost = 600  },
        new () { RankName = "Legend",       CashMultiplier = 1.7f, TrophiesCost = 700  },
        new () { RankName = "God",          CashMultiplier = 1.8f, TrophiesCost = 800  },
        new () { RankName = "Immortal",     CashMultiplier = 1.9f, TrophiesCost = 900  },
        new () { RankName = "Eternal",      CashMultiplier = 2.0f, TrophiesCost = 1000 },
        new () { RankName = "Infinite",     CashMultiplier = 2.1f, TrophiesCost = 1100 },
    };

    public RebirthData GetRebirthData(FatPlayer player)
    {
        var rebirth = player.Rebirth;
        if (rebirth >= RebirthData.Count)
            rebirth = RebirthData.Count - 1;
        return RebirthData[rebirth];
    }
}

public class RebirthData
{
    public string RankName;
    public float CashMultiplier;
    public int TrophiesCost;
}