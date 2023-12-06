using AO;

public class FatGameManager : Component
{
    public override void Awake()
    {
        Leaderboard.RegisterSortCallback((Player[] players) =>
        {
            Array.Sort(players, (a, b) =>
            {
                return ((FatPlayer)b).Rebirth.CompareTo(((FatPlayer)a).Rebirth);
            });
        });

        Leaderboard.Register("Rank", (Player[] players, string[] scores) =>
        {
            for (int i = 0; i < players.Length; i++)
            {
                FatPlayer fatPlayer = (FatPlayer)players[i];
                var p = fatPlayer.Entity.Position;
                var rbd = Rebirth.Instance.GetRebirthData(fatPlayer.Rebirth);
                scores[i] = rbd.RankName;
            }
        });

        Leaderboard.Register("Trophies", (Player[] players, string[] scores) =>
        {
            for (int i = 0; i < players.Length; i++)
            {
                FatPlayer fatPlayer = (FatPlayer)players[i];
                scores[i] = fatPlayer.Trophies.ToString();
            }
        });
    }
}