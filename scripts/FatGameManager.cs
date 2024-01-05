using AO;

public class FatGameManager : Component
{
    public static FatGameManager Instance;

    public bool CheckAdmin(Player player)
    {
        if (!player.IsAdmin)
        {
            Chat.SendMessage("You must be an admin to use this command.", player);
            return false;
        }
        return true;
    }

    public void RunChatCommand(Player p, string command)
    {
        var parts = command.ToLowerInvariant().Split(' ');
        var cmd = parts[0];
        FatPlayer player = (FatPlayer)p;
        switch (cmd)
        {
            case "grant":
            {
                if (!CheckAdmin(player)) return;

                if (parts.Length < 4)
                {
                    Chat.SendMessage("Usage: /grant <player> <item> [amount]", player);
                    return;
                }

                var target = Player.AllPlayers.FirstOrDefault(p => p.Name.ToLowerInvariant() == parts[1]);
                if (parts[1] == "self" || parts[1] == "me")
                {
                    target = player;
                }

                if (target == null)
                {
                    Chat.SendMessage($"Grant failed, player {parts[1]} not found.", player);
                    return;
                }

                switch (parts[2])
                {
                    case "coins":
                    {
                        var amount = 1;
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[3], out amount);
                        }

                        var fatTarget = (FatPlayer)target;
                        fatTarget.Coins += amount;
                        return;
                    }

                    case "trophies":
                    {
                        var amount = 1;
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[3], out amount);
                        }

                        var fatTarget = (FatPlayer)target;
                        fatTarget.GiveTrophies(amount, true, fatTarget.Entity.Position, fatTarget.Entity);
                        return;
                    }
                }

                var item = ShopData.Items.FirstOrDefault(i => i.Name.ToLowerInvariant() == parts[2]);
                if (item == null)
                {
                    Chat.SendMessage($"Grant failed, item {parts[2]} not found.", player);
                    return;
                }

                if (item.Currency == ShopData.Currency.Sparks)
                {
                    var product = Purchasing.GetProduct(item.ProductId);
                    if (product.IsValid() && product.IsGamePass)
                    {
                        Chat.SendMessage($"Grant failed, item {parts[2]} cannot grant game passes.", player);
                        return;
                    }
                }

                Shop.Instance.GrantItem(target, item);
                break;
            }
            case "reset_progress":
            {
                if (!CheckAdmin(player)) return;
                player.ResetProgressCheat();
                break;
            }
            case "+money":
            {
                if (!CheckAdmin(player)) return;
                player.IncreaseMoneyCheat();
                break;
            }
            case "-money":
            {
                if (!CheckAdmin(player)) return;
                player.DecreaseMoneyCheat();
                break;
            }
            case "+trophies":
            {
                if (!CheckAdmin(player)) return;
                player.IncreaseTrophiesCheat();
                break;
            }
            case "-trophies":
            {
                if (!CheckAdmin(player)) return;
                player.DecreaseTrophiesCheat();
                break;
            }
            case "complete_quest":
            {
                if (!CheckAdmin(player)) return;
                player.ForceCompleteQuestCheat();
                break;
            }
            default:
            {
                Chat.SendMessage($"Unknown chat command '{cmd}'", player);
                return;
            }
        }
    }

    public override void Awake()
    {
        Instance = this;

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

        Player.OnPlayerJoin += (Player player) =>
        {
            List<ulong> disabledFoods = new List<ulong>();
            foreach (var food in Food.AllFoods)
            {
                if (!food.WasDynamicallySpawned && food.HasBeenEaten)
                {
                    disabledFoods.Add(food.Entity.NetworkId);
                }
            }
            ((FatPlayer)player).CallClient_DisableInSceneFoods(disabledFoods.ToArray());
        };

        Chat.RegisterChatCommandHandler(RunChatCommand);
    }
}