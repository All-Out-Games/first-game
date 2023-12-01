using AO;

public class FatGameManager : Component
{
    public override void Awake()
    {
        Leaderboard.RegisterSortCallback((Player[] players) =>
        {
            Array.Sort(players, (a, b) =>
            {
                return ((FatPlayer)b).Food.CompareTo(((FatPlayer)a).Food);
            });
        });

        Leaderboard.Register("Food", (Player[] players, string[] scores) =>
        {
            for (int i = 0; i < players.Length; i++)
            {
                FatPlayer fatPlayer = (FatPlayer)players[i];
                scores[i] = fatPlayer.Food.ToString();
            }
        });

        Leaderboard.Register("Position", (Player[] players, string[] scores) =>
        {
            for (int i = 0; i < players.Length; i++)
            {
                FatPlayer fatPlayer = (FatPlayer)players[i];
                var p = fatPlayer.Entity.Position;
                scores[i] = $"({p.X:F1}, {p.Y:F1})";
            }
        });
    }
}