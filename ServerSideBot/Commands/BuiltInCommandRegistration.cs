using System.Linq;
using TShockAPI;

namespace ServerSideBot.Commands
{
    public class BuiltInCommandRegistration
    {
        public static void RegisterCommands()
        {
            SSBot.chatHandler.RegisterCommand("bot.staff.admin", AdminUser, "admin");
            SSBot.chatHandler.RegisterCommand("bot.staff.vip", VipUser, "vip");
            SSBot.chatHandler.RegisterCommand("bot.ignore", IgnoreUser, "ignore");
            SSBot.chatHandler.RegisterCommand("bot.find", FindUsers, "find");
            SSBot.chatHandler.RegisterCommand("bot.say", SayBot, "say");
            SSBot.chatHandler.RegisterCommand("bot.help", HelpCmd, "help");

            #region Channelling Commands

            SSBot.chatHandler.RegisterCommand("bot.channel", Channel, "channel", "c");

            #endregion
        }

        #region Channelling Commands

        private static bool Channel(_CommandArgs args)
        {

            if (args.Parameters.Count == 0)
            {
                SendIRCHelp(args.Player.TSPlayer);
                return true;
            }
            switch (args.Parameters[0].ToLower())
            {
                    #region Join

                case "join":
                {
                    if (args.Parameters.Count < 2)
                    {
                        SendIRCHelp(args.Player.TSPlayer, "join");
                        return true;
                    }

                    var channelName = args.Parameters[1].StartsWith("#")
                        ? args.Parameters[1]
                        : "#" + args.Parameters[1];

                    var channels = SSBot.channelManager.GetChannelsByName(channelName);

                    if (channels.Count > 1)
                    {
                        args.Bot.PrivateSay("Multiple channel matches found: ", args.Player.TSPlayer);
                        args.Bot.PrivateSay(string.Join(", ", channels.Select(c => c.name)),
                            args.Player.TSPlayer);
                        return true;
                    }
                    if (channels.Count == 0)
                    {
                        args.Bot.PrivateSay("No channels matched query '{0}'", args.Player.TSPlayer, channelName);
                        args.Bot.PrivateSay("Creating channel '{0}'", args.Player.TSPlayer, channelName);
                        SSBot.channelManager.CreateChannel(channelName, args.Player);
                        return true;
                    }

                    var channel = channels[0];
                    channel.AttemptJoin(args.Player);
                    return true;
                }

                    #endregion

                    #region Part

                case "part":
                {
                    if (args.Player.Channel != SSBot.globalChannel)
                    {
                        var part = args.Parameters.Count > 1
                            ? args.Parameters[1]
                            : "part";

                        var channel = args.Player.Channel;
                        args.Bot.PrivateSay("Disconnected from {0}",
                            args.Player.TSPlayer, channel.name);

                        args.Player.Channel.Part(args.Player);

                        channel.SendMessage("{0} left ({1})", args.Player.name, part);
                        return true;
                    }
                    if (!args.Player.partConfirmed)
                    {
                        args.Bot.PrivateSay("Parting the global channel results in exitting the server.",
                            args.Player.TSPlayer);
                        args.Bot.PrivateSay("Part again to confirm.",
                            args.Player.TSPlayer);
                        args.Player.partConfirmed = true;
                        return true;
                    }

                    args.Player.partConfirmed = false;
                    var reason = args.Parameters.Count > 0
                        ? "Part: " + string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1))
                        : "Part: (Disconnected by peer)";
                    args.Player.TSPlayer.Disconnect(reason);
                    return true;
                }

                    #endregion

                    #region Mode

                case "mode":
                {
                    if (args.Parameters.Count < 2)
                    {
                        SendIRCHelp(args.Player.TSPlayer, "mode");
                        return true;
                    }

                    var channelName = args.Parameters[1].StartsWith("#")
                        ? args.Parameters[1]
                        : "#" + args.Parameters[1];

                    var channels = SSBot.channelManager.GetChannelsByName(channelName);

                    if (channels.Count > 1)
                    {
                        args.Bot.PrivateSay("Multiple channel matches found: ", args.Player.TSPlayer);
                        args.Bot.PrivateSay(string.Join(", ", channels.Select(c => c.name)),
                            args.Player.TSPlayer);
                        return true;
                    }
                    if (channels.Count == 0)
                    {
                        args.Bot.PrivateSay("No channels matched query '{0}'",
                            args.Player.TSPlayer, channelName);
                        return true;
                    }

                    var channel = channels[0];
                    var flags = string.Join("", args.Parameters);
                    var add = flags.StartsWith("+");
                    for (var i = 0; i < flags.Length; i++)
                    {
                        var c = flags[i];
                        if (i == flags.Length - 1)
                        {
                            if (add)
                                channel.AddFlag(c, true);
                            else
                                channel.DelFlag(c, true);
                        }
                        else
                        {
                            if (add)
                                channel.AddFlag(c);
                            else
                                channel.DelFlag(c);
                        }
                    }
                    args.Bot.PrivateSay("{0} now has modes: {1}", args.Player.TSPlayer,
                        channel.name, channel.modes);

                    return true;
                }

                    #endregion

                    #region Help

                case "help":
                {
                    if (args.Parameters.Count < 2)
                    {
                        SendIRCHelp(args.Player.TSPlayer);
                        return true;
                    }
                    SendIRCHelp(args.Player.TSPlayer, args.Parameters[1]);
                    return true;
                }

                    #endregion

                    #region Invite

                case "invite":
                {
                    return true;
                }

                    #endregion

                    #region Kick

                case "kick":
                {
                    return true;
                }

                    #endregion

                    #region Topic

                case "topic":
                {
                    return true;
                }

                    #endregion

                    #region Ban

                case "ban":
                {
                    return true;
                }

                    #endregion

                    #region List

                case "list":
                {
                    if (args.Parameters.Count > 1)
                    {
                        var searchStr = string.Join(" ", args.Parameters);
                        var channels = SSBot.channelManager.GetChannelsByName(searchStr);

                        args.Bot.PrivateSay("{0} matches found: ", args.Player.TSPlayer, channels.Count);
                        args.Bot.PrivateSay(string.Join(", ", channels.Select(c => c.name)),
                            args.Player.TSPlayer);

                        return true;
                    }

                    args.Bot.PrivateSay("{0} channel{1} exist{2}: ", args.Player.TSPlayer,
                        SSBot.channelManager.Channels.Count,
                        SSBot.channelManager.Channels.Count > 1 || SSBot.channelManager.Channels.Count == 0
                            ? "s"
                            : "",
                        SSBot.channelManager.Channels.Count > 1 || SSBot.channelManager.Channels.Count == 0
                            ? ""
                            : "s");
                    args.Bot.PrivateSay(string.Join(", ", SSBot.channelManager.Channels.Select(c => c.name)),
                        args.Player.TSPlayer);

                    return true;
                }

                    #endregion
            }
            return false;
        }

        #endregion

        #region Help Command

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
                PaginationTools.BuildLinesFromTerms(SSBot.chatHandler.Commands.Select(c => c.Names[0])),
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

        #endregion

        #region SayBot Command

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

        #endregion

        #region FindUsers Command

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

        #endregion

        #region AdminUser Command

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

        #endregion

        #region VipUser Command

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

        #endregion

        #region IgnoreUser Command

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

        #endregion

        #region SendIRCHelp

        private static void SendIRCHelp(TSPlayer player, string command = null)
        {
            if (command == null)
            {
                player.SendInfoMessage("Channel Commands  ([text] - required | {text} - optional):");
                player.SendInfoMessage(
                    "join #channelName {password} - Attempt to join a channel. Private channels require passwords");
                player.SendInfoMessage("part #channelName {msg} - Leave a channel with an optional message");
                player.SendInfoMessage("mode #channelName [flags] - Set flags on a channel");
                player.SendInfoMessage("invite [username] - Invites a user to the channel you're currently in");
                player.SendInfoMessage("kick [username] - Removes a user from your current channel");
                player.SendInfoMessage("topic [text] - Sets the greet message of your channel");
                player.SendInfoMessage("ban [username] - Prevents a user from joining your channel");
                player.SendInfoMessage("access [username] [accesstype] - Grant users privelidges in a channel");
                player.SendInfoMessage("list - Lists available channels");
                return;
            }

            switch (command.ToLower())
            {
                case "join":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c join channel {password}",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Channel must follow the format '#channelName'");
                    player.SendInfoMessage("Passwords are only required for password protected channels");
                    player.SendSuccessMessage("EG: {0)c join #staff staffchat <- passworded",
                        SSBot.Config.CommandCharacter);
                    player.SendSuccessMessage("EG: {0}c join #users <- non-passworded",
                        SSBot.Config.CommandCharacter);
                    return;

                case "part":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c part {part message}",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Parting a channel causes you to disconnect from it.");
                    player.SendInfoMessage(
                        "If you include a message, that message will be sent to the channel you leave.");
                    player.SendSuccessMessage("EG: {0}c part bye guys!",
                        SSBot.Config.CommandCharacter);
                    return;

                case "mode":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c mode channel [flags]",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Channel modes effect the channel:");
                    player.SendInfoMessage("s -> channel is private. m -> channel is muted");
                    player.SendInfoMessage(
                        "i -> channel requires invitation to join. r -> users must be registered to join");
                    player.SendInfoMessage(
                        "k {password} -> channel has password {password} set. l {number} -> only {number} of people can join channel");
                    player.SendSuccessMessage("EG: {0}c mode #staff sir <- set modes 's', 'i' and 'r'",
                        SSBot.Config.CommandCharacter);
                    player.SendSuccessMessage("EG: {0}c mode #staff k staffchat <- set password 'staffchat'",
                        SSBot.Config.CommandCharacter);
                    return;

                case "invite":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c invite [playerName]",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Invites a user to join your current channel.");
                    player.SendInfoMessage("Player can then join the channel with {0}c join",
                        SSBot.Config.CommandCharacter);
                    player.SendSuccessMessage("EG: {0}c invite WhiteX",
                        SSBot.Config.CommandCharacter);
                    return;

                case "kick":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c kick [username]",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Kicks a player from your current channel");
                    player.SendSuccessMessage("EG: {0}c kick WhiteX",
                        SSBot.Config.CommandCharacter);
                    return;
                case "topic":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c topic {topic}",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Sends the current channel's topic.");
                    player.SendInfoMessage(
                        "If {topic} is defined and you have permission, changes the channel's topic to {topic}",
                        SSBot.Config.CommandCharacter);
                    player.SendSuccessMessage("EG: {0)c topic <- displays topic",
                        SSBot.Config.CommandCharacter);
                    player.SendSuccessMessage("EG: {0}c topic new topic for #staff! <- sets topic",
                        SSBot.Config.CommandCharacter);
                    return;

                case "ban":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c ban [username] {ban message}",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Bans a player from your current channel.");
                    player.SendSuccessMessage("EG: {0}c ban WhiteX",
                        SSBot.Config.CommandCharacter);
                    return;

                case "accesss":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c access [username] [access level]",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Grants a user certain privelidges in your current channel");
                    player.SendInfoMessage("Access Levels range from 0-5");
                    player.SendInfoMessage("0 - default. 1 - can talk while channel is muted");
                    player.SendInfoMessage("2 - can join without invite. 3 - can invite to channel & manage topic");
                    player.SendInfoMessage("4 - can manage users in channel. 5 - full access");
                    player.SendSuccessMessage("EG: {0}c access WhiteX 5",
                        SSBot.Config.CommandCharacter);
                    return;

                case "list":
                    player.SendWarningMessage("[Channel Help] Syntax: {0}c list {search term}",
                        SSBot.Config.CommandCharacter);
                    player.SendInfoMessage("Sends a list of channels.");
                    player.SendInfoMessage("If {search term} is defined, sends a list of relevant channels");
                    player.SendSuccessMessage("EG: {0c list <- lists all channels",
                        SSBot.Config.CommandCharacter);
                    player.SendSuccessMessage("EG: {0}c list st <- lists all channels that contains 'st'",
                        SSBot.Config.CommandCharacter);
                    return;

                default:
                    player.SendInfoMessage("Channel Commands  ([text] - required | {text} - optional):");
                    player.SendInfoMessage(
                        "join #channelName {password} - Attempt to join a channel. Private channels require passwords");
                    player.SendInfoMessage("part #channelName {msg} - Leave a channel with an optional message");
                    player.SendInfoMessage("mode #channelName [flags] - Set flags on a channel");
                    player.SendInfoMessage("invite [username] - Invites a user to the channel you're currently in");
                    player.SendInfoMessage("kick [username] - Removes a user from your current channel");
                    player.SendInfoMessage("topic [text] - Sets the greet message of your channel");
                    player.SendInfoMessage("ban [username] - Prevents a user from joining your channel");
                    player.SendInfoMessage("access [username] [access level] - Grant users privelidges in a channel");
                    player.SendInfoMessage("list - Lists available channels");
                    return;
            }
        }

        #endregion
    }
}
