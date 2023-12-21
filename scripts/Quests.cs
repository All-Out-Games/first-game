using AO;
using TinyJson;

/*
TODO(JOSH):
-split up quests by world index
-spawn carepackages in all worlds? just one world at a time? cycling? idk what the move is, ask arjan
*/

public partial class CarePackage : Component
{
    public bool Claimed;

    public override void Start()
    {
        var interactable = Entity.GetComponent<Interactable>();
        interactable.OnInteract = (Player p) =>
        {
            var player = (FatPlayer)p;
            if (Claimed)
            {
                if (player.IsLocal)
                {
                    Notifications.Show("Care Package has already been claimed. Better luck next time!");
                }
                return;
            }
            if (player.CurrentQuest != null)
            {
                if (player.IsLocal)
                {
                    Notifications.Show("You already have a quest!");
                }
                return;
            }

            if (Network.IsServer)
            {
                QuestType type = Quest.GetRandomQuestType();
                CallClient_OnClaimed(p.Entity.NetworkId, (int)type);
                Network.Despawn(Entity);
                Entity.Destroy();
            }
        };

        interactable.CanUseCallback = (Player p) =>
        {
            var player = (FatPlayer)p;
            if (Claimed)
            {
                return false;
            }
            return true;
        };
    }

    [ClientRpc]
    public void OnClaimed(ulong playerNetworkId, int _questType)
    {
        var questType = (QuestType)_questType;
        var player = Entity.FindByNetworkId(playerNetworkId).GetComponent<FatPlayer>();
        if (player == null) return;

        switch (questType)
        {
            case QuestType.SpeedEater:     { player.CurrentQuest = new SpeedEaterQuest();     break; }
            case QuestType.UniqueFoods:    { player.CurrentQuest = new UniqueFoodsQuest();    break; }
            case QuestType.SequencedEater: { player.CurrentQuest = new SequencedEaterQuest(); break; }
            case QuestType.BigBoiDiet:     { player.CurrentQuest = new BigBoiDietQuest();     break; }
            case QuestType.MarathonEater:  { player.CurrentQuest = new MarathonEaterQuest();  break; }
            case QuestType.BossBrawl:      { player.CurrentQuest = new BossBrawlQuest();      break; }
            // case QuestType.TheGoldenApple: { player.CurrentQuest = new TheGoldenAppleQuest(); break; }
            case QuestType.Trackstar:      { player.CurrentQuest = new TrackstarQuest();      break; }
            default: {
                Log.Error("Unknown quest type: " + questType);
                return;
            }
        }

        player.CurrentQuest.TimeLeft = player.CurrentQuest.QuestTime;
        player.CurrentQuest.Progress = 0;
        player.CurrentQuest.Player = player;
        if (Network.IsServer)
        {
            player.CurrentQuest.InitServer();
        }
        Claimed = true;
    }
}

public enum QuestType
{
    SpeedEater,
    UniqueFoods,
    SequencedEater,
    BigBoiDiet,
    MarathonEater,
    BossBrawl,
    // TheGoldenApple,
    Trackstar,

    COUNT,
}

public class Quest
{
    public QuestType Type;

    public static QuestType GetRandomQuestType()
    {
        var questType = (QuestType)(new Random().Next((int)QuestType.COUNT));
        return questType;
    }

    public FatPlayer Player;

    public virtual string QuestName => "An Quest";
    public virtual string Objective => "Do the thing.";
    public virtual string RewardDescription => "A boatload of money!";
    public virtual float QuestTime => 30;
    public virtual int ProgressRequired => 5;
    public virtual void OnFoodEatenServer(Food food) { }
    public virtual void OnBossBeatenServer(Boss boss) { }
    public virtual void OnSoldItemsServer() { }
    public virtual void GiveRewards() { }

    public float TimeLeft;
    public int Progress;

    public virtual void InitServer() { }
    public virtual bool UpdateServer()
    {
        Util.Assert(Network.IsServer);
        TimeLeft -= Time.DeltaTime;
        Player.CallClient_SyncQuestTimeLeft(TimeLeft);
        if (TimeLeft <= 0)
        {
            QuestFailed("Ran out of time.");
            return false;
        }
        return true;
    }

    public void ReportProgress(int progress)
    {
        Util.Assert(Network.IsServer);
        Player.CallClient_OnQuestProgressUpdated(progress);
        if (progress >= ProgressRequired)
        {
            Player.CallClient_OnQuestFinished(true, string.Empty);
            GiveRewards();
        }
    }

    public void QuestFailed(string reason)
    {
        Util.Assert(Network.IsServer);
        Player.CallClient_OnQuestFinished(false, reason);
    }
}

public class SpeedEaterQuest : Quest
{
    public override string QuestName => "Speed Eater";
    public override string Objective => "Quick! Eat 30 things in under 2 minutes.";
    public override string RewardDescription => "150 Coins.";
    public override float QuestTime => 2 * 60;
    public override int ProgressRequired => 30;

    public override void GiveRewards()
    {
        Player.Coins += 150;
    }

    public int ThingsEaten;
    public override void OnFoodEatenServer(Food food)
    {
        Util.Assert(Network.IsServer);
        ThingsEaten += 1;
        ReportProgress(ThingsEaten);
    }
}

public class UniqueFoodsQuest : Quest
{
    public override string QuestName => "Unique Foods";
    public override string Objective => "Eat 7 unique things in under 1 minute!";
    public override string RewardDescription => "2x Cash Multiplier for 60 seconds.";
    public override float QuestTime => 1 * 60;
    public override int ProgressRequired => 7;

    public override void GiveRewards()
    {
        Player.ServerGiveTemporaryBuff(StatModifierKind.CashMultiplier, 2.0f, 60);
    }

    public HashSet<FoodDefinition> FoodsEaten = new();
    public override void OnFoodEatenServer(Food food)
    {
        Util.Assert(Network.IsServer);
        FoodsEaten.Add(food.Definition);
        ReportProgress(FoodsEaten.Count);
    }
}

public class SequencedEaterQuest : Quest
{
    public override string QuestName => "Sequenced Eater";
    public override string Objective => "Don't eat the same food in a row for 30 seconds! (minimum 5 foods eaten)";
    public override string RewardDescription => "2x Click Power for 60 seconds.";
    public override float QuestTime => 30;
    public override int ProgressRequired => 5;

    public override void GiveRewards()
    {
        Player.ServerGiveTemporaryBuff(StatModifierKind.ClickPower, 2.0f, 60);
    }

    public HashSet<FoodDefinition> FoodsEaten = new();
    public override void OnFoodEatenServer(Food food)
    {
        Util.Assert(Network.IsServer);
        if (FoodsEaten.Contains(food.Definition))
        {
            QuestFailed($"You already ate a {food.Definition.Name}.");
            return;
        }
        FoodsEaten.Add(food.Definition);
        ReportProgress(FoodsEaten.Count);
    }
}

public class BigBoiDietQuest : Quest
{
    public override string QuestName => "Big Boi Diet";
    public override string Objective => "Eat only foods size 10 or higher for 30 seconds! (minimum 5 foods eaten)";
    public override string RewardDescription => "2x Stomach Size for 5 minutes.";
    public override float QuestTime => 30;
    public override int ProgressRequired => 5;

    public override void GiveRewards()
    {
        Player.ServerGiveTemporaryBuff(StatModifierKind.StomachSize, 2.0f, 5 * 60);
    }

    public int ThingsEaten;
    public override void OnFoodEatenServer(Food food)
    {
        Util.Assert(Network.IsServer);
        if (food.Definition.RequiredMouthSize < 10)
        {
            QuestFailed($"{food.Definition.Name} has a required mouth size less than 10.");
            return;
        }
        ThingsEaten += 1;
        ReportProgress(ThingsEaten);
    }
}

public class MarathonEaterQuest : Quest
{
    public override string QuestName => "Marathon Eater";
    public override string Objective => "Eat 100 things in under 7 minutes!";
    public override string RewardDescription => "300 Coins.";
    public override float QuestTime => 7 * 60;
    public override int ProgressRequired => 100;

    public override void GiveRewards()
    {
        Player.Coins += 300;
    }

    public int ThingsEaten;
    public override void OnFoodEatenServer(Food food)
    {
        Util.Assert(Network.IsServer);
        ThingsEaten += 1;
        ReportProgress(ThingsEaten);
    }
}

public class BossBrawlQuest : Quest
{
    public override string QuestName => "Boss Brawl";
    public override string Objective => "Defeat each boss in the world once in under 1 minute!";
    public override string RewardDescription => "2000 Trophies.";
    public override float QuestTime => 1 * 60;
    public override int ProgressRequired => 5;

    public override void GiveRewards()
    {
        Player.Trophies += 2000;
    }

    public HashSet<BossDefinition> World1BossesDefeated = new();
    public override void OnBossBeatenServer(Boss boss)
    {
        Util.Assert(Network.IsServer);
        if (boss.Definition.WorldIndex != 0)
        {
            return;
        }
        World1BossesDefeated.Add(boss.Definition);
        ReportProgress(World1BossesDefeated.Count);
    }
}

// public class TheGoldenAppleQuest : Quest
// {
//     public override string QuestName => "The Golden Apple";
//     public override string Objective => "Find the golden apple and sell it in less than 30 seconds!";
//     public override string RewardDescription => "1000 Coins.";
//     public override float QuestTime => 30;
//     public override int ProgressRequired => 1;

//     public override void GiveRewards()
//     {
//         Player.Coins += 1000;
//     }

//     public bool Found;

//     public override void OnFoodEatenServer(Food food)
//     {
//         Util.Assert(Network.IsServer);
//         if (food.Definition.Id == "golden_apple")
//         {
//             Found = true;
//         }
//     }

//     public override void OnSoldItemsServer()
//     {
//         Util.Assert(Network.IsServer);
//         if (Found)
//         {
//             ReportProgress(1);
//         }
//     }
// }

public class TrackstarQuest : Quest
{
    public override string QuestName => "Trackstar";
    public override string Objective => "Run 1000 feet in under 5 minutes!";
    public override string RewardDescription => "2x Move Speed for 10 minutes.";
    public override float QuestTime => 5 * 60;
    public override int ProgressRequired => 1000;

    public Vector2 PositionLastFrame;
    public float TotalDistanceTraveledMeters;

    public override void GiveRewards()
    {
        Player.ServerGiveTemporaryBuff(StatModifierKind.MoveSpeed, 2.0f, 10 * 60);
    }

    public override void InitServer()
    {
        PositionLastFrame = Player.Entity.Position;
    }

    public override bool UpdateServer()
    {
        if (!base.UpdateServer())
        {
            return false;
        }
        if ((Time.TimeSinceStartup - Player.TimeLastTeleported) >= 0.5) // huge hack but good enough for now
        {
            float traveled = (PositionLastFrame - Player.Entity.Position).Length;
            TotalDistanceTraveledMeters += traveled;
            float totalFeetTraveled = TotalDistanceTraveledMeters * 3.28084f;
            ReportProgress((int)totalFeetTraveled);
        }
        PositionLastFrame = Player.Entity.Position;
        return true;
    }
}
