using System.Linq;
using TShockAPI;

namespace ServerSideBot.Commands
{
    public class BuiltInCommandRegistration
    {
        public static void RegisterCommands()
        {
            SSBot.ChatHandler.RegisterCommand("bot.staff.admin", AdminUser, "admin");
            SSBot.ChatHandler.RegisterCommand("bot.staff.vip", VipUser, "vip");
            SSBot.ChatHandler.RegisterCommand("bot.ignore", IgnoreUser, "ignore");
            SSBot.ChatHandler.RegisterCommand("bot.find", FindUsers, "find");
            SSBot.ChatHandler.RegisterCommand("bot.say", SayBot, "say");
            SSBot.ChatHandler.RegisterCommand("bot.help", HelpCmd, "help");
        }

        private static bool HelpCmd(_CommandArgs args)
        {
            var page = 1;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player.TSPlayer, out page))
            {
                args.Bot.PrivateSay("Invalid usage. Try {0}help [number]", args.Player.TSPlayer,
                   SSBot.Config.CommandCharacter);
                return false;
            }

            PaginationTools.SendPage(args.Player.TSPlayer, page, 
                PaginationTools.BuildLinesFromTerms(SSBot.ChatHandler.Commands.Select(c => c.Names[0])),
                new PaginationTools.Settings
                {
                    HeaderFormat = "Bot commands ({0}/{1}):",
                    HeaderTextColor = Color.Green,
                    FooterTextColor = Color.Yellow,
                    LineTextColor = Color.Yellow,
                    FooterFormat = string.Format("Type{0}help", SSBot.Config.CommandCharacter) + " {0} for more",
                    IncludeFooter = true,
                    NothingToDisplayString = "No Commands functioning!"
                });
            return true;
        }

        private static bool SayBot(_CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Bot.PrivateSay("Invalid usage. Try {0}say text", args.Player.TSPlayer,
                    SSBot.Config.CommandCharacter);
                return false;
            }

            var text = string.Join(" ", args.Parameters);
            args.Bot.Say(text);
            return true;
        }

        private static bool FindUsers(_CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Bot.PrivateSay("Invalid usage. Try {0}find [player]", args.Player.TSPlayer,
                    SSBot.Config.CommandCharacter);
                return false;
            }

            var user = string.Join(" ", args.Parameters);
            var players = SSBot.Storage.GetPlayerListByName(user);

            if (players.Count > 1)
            {
                if (!args._private)
                    args.Bot.Say("{0} matches found: {1}.", players.Count,
                        string.Join(", ", players.Select(p => p.name)));
                else
                    args.Bot.PrivateSay("{0} matches found: {1}.", args.Player.TSPlayer, players.Count,
                        string.Join(", ", players.Select(p => p.name)));
                return true;
            }
            if (players.Count == 0)
            {
                if (!args._private)
                    args.Bot.Say("0 matches found for query '{0}'.", user);
                else
                    args.Bot.PrivateSay("0 matches found for query '{0}'.", args.Player.TSPlayer, user);
                return true;
            }

            if (!args._private)
                args.Bot.Say("1 match found: {0}.", players[0].name);
            else
                args.Bot.PrivateSay("1 match found: {0}.", args.Player.TSPlayer, players[0].name);
            return true;
        }

        private static bool AdminUser(_CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var user = string.Join(" ", args.Parameters);
                var players = SSBot.Storage.GetTSUsersListByName(user);
                if (players.Count > 1)
                {
                    args.Bot.PrivateSay("{0} matches found: {1}.", args.Player.TSPlayer, players.Count,
                        string.Join(", ", players.Select(p => p.Name)));
                    return false;
                }
                if (players.Count == 0)
                {
                    args.Bot.PrivateSay("No matches found for '{0}'.", args.Player.TSPlayer, user);
                    return false;
                }

                var player = players[0];
                TShock.Users.SetUserGroup(player, "admin");
                if (!args._private)
                    args.Bot.Say("Group 'admin' assigned to {0}.", player.Name);
                else
                    args.Bot.PrivateSay("Group 'admin' assigned to {0}.", args.Player.TSPlayer, player.Name);

                return true;
            }

            TShock.Users.SetUserGroup(
                TShock.Users.GetUserByName(args.Player.TSPlayer.UserAccountName), "admin");
            args.Bot.PrivateSay("Group 'admin' assigned to self.", args.Player.TSPlayer);
            return true;
        }

        private static bool VipUser(_CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var user = string.Join(" ", args.Parameters);
                var players = SSBot.Storage.GetTSUsersListByName(user);
                if (players.Count > 1)
                {
                    args.Bot.PrivateSay("{0} matches found: {1}.", args.Player.TSPlayer, players.Count,
                        string.Join(", ", players.Select(p => p.Name)));
                    return false;
                }
                if (players.Count == 0)
                {
                    args.Bot.PrivateSay("No matches found for '{0}'.", args.Player.TSPlayer, user);
                    return false;
                }

                var player = players[0];
                TShock.Users.SetUserGroup(player, "vip");
                if (!args._private)
                    args.Bot.Say("Group 'vip' assigned to {0}.", player.Name);
                else
                    args.Bot.PrivateSay("Group 'vip' assigned to {0}.", args.Player.TSPlayer, player.Name);

                return true;
            }

            TShock.Users.SetUserGroup(
                TShock.Users.GetUserByName(args.Player.TSPlayer.UserAccountName), "vip");
            args.Bot.PrivateSay("Group 'vip' assigned to self.", args.Player.TSPlayer);
            return true;
        }

        private static bool IgnoreUser(_CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Bot.PrivateSay("Invalid usage. Try {0}ignore [player]", args.Player.TSPlayer,
                    SSBot.Config.CommandCharacter);
                return false;
            }

            var user = string.Join(" ", args.Parameters).ToLower();

            if (user == "list")
            {
                args.Bot.PrivateSay("Your ignored list has {0} user{1} in it: ", args.Player.TSPlayer,
                    args.Player.ignoredPlayers.Count,
                    args.Player.ignoredPlayers.Count > 1 || args.Player.ignoredPlayers.Count == 0 ? "s" : "");
                args.Bot.PrivateSay(string.Join(", ", args.Player.ignoredPlayers), args.Player.TSPlayer);

                return true;
            }

            if (args.Player.ignoredPlayers.Contains(user))
            {
                args.Player.ignoredPlayers.RemoveEx(args.Player, user);
                args.Bot.PrivateSay("{0} has been removed from your ignore list.", args.Player.TSPlayer,
                    user);
                return true;
            }

            BPlayer player = null;
            var players = SSBot.Storage.GetPlayerListByName(user);
            if (players.Count > 1)
            {
                args.Bot.PrivateSay("{0} matches found: {1}.", args.Player.TSPlayer, players.Count,
                        string.Join(", ", players.Select(p => p.name)));
                return false;
            }
            if (players.Count == 0)
            {
                args.Bot.PrivateSay("No matches found for '{0}'.", args.Player.TSPlayer, user);
                return false;
            }

            player = players[0];

            if (player == null)
            {
                args.Bot.PrivateSay("No matches found for '{0}'.", args.Player.TSPlayer, user);
                return false;
            }

            if (player == args.Player)
            {
                args.Bot.PrivateSay("Ignoring yourself is not advisable.", args.Player.TSPlayer);
                return false;
            }


            if (args.Player.ignoredPlayers.Contains(player.name))
            {
                args.Player.ignoredPlayers.RemoveEx(args.Player, player.name);
                args.Bot.PrivateSay("{0} has been removed from your ignore list.", args.Player.TSPlayer,
                    player.name);
                return true;
            }
            args.Player.ignoredPlayers.AddEx(args.Player, player.name);
            args.Bot.PrivateSay("{0} has been added to your ignore list.", args.Player.TSPlayer,
                player.name);
            return true;
        }
    }
}
