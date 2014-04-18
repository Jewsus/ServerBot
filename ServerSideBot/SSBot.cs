using System;
using System.Globalization;
using System.IO;
using System.Data;
using System.Linq;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using ServerSideBot.Commands;
using Terraria;
using TerrariaApi.Server;

using TShockAPI;
using TShockAPI.Hooks;

namespace ServerSideBot
{
    [ApiVersion(1, 15)]
    public class SSBot : TerrariaPlugin
    {
        private IDbConnection _db;

        public static DManager Database;
        public static Storage Storage = new Storage();
        public static Config Config = new Config();
        public static Bot Bot = new Bot();
        public readonly static ChatHandler chatHandler = new ChatHandler();
        public readonly static ChannelManager channelManager = new ChannelManager();
        public static GlobalChannel globalChannel = new GlobalChannel();

        private bool postInitialized;

        public override string Author
        {
            get { return "White & Ijdawg"; }
        }

        public override string Description
        {
            get { return "Attempts to emulate an IRC-like bot, offering short commands and the like"; }
        }

        public override string Name
        {
            get { return "ServerSide Bot"; }
        }

        public override Version Version
        {
            get { return new Version(0, 1); }
        }

        #region Initialize
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerChat.Register(this, OnChat, 1);
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet, -5);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            PlayerHooks.PlayerPostLogin += PostLogin;

            #region Database stuff

            switch (TShock.Config.StorageType.ToLower())
            {
                case "sqlite":
                    _db =
                        new SqliteConnection(string.Format("uri=file://{0},Version=3",
                            Path.Combine(TShock.SavePath, "BotData.sqlite")));
                    break;

                case "mysql":
                    try
                    {
                        var host = TShock.Config.MySqlHost.Split(':');
                        _db = new MySqlConnection
                        {
                            ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                                host[0],
                                host.Length == 1 ? "3306" : host[1],
                                TShock.Config.MySqlDbName,
                                TShock.Config.MySqlUsername,
                                TShock.Config.MySqlPassword
                                )
                        };
                    }
                    catch (MySqlException x)
                    {
                        Log.Error(x.ToString());
                        throw new Exception("MySQL not setup correctly.");
                    }
                    break;

                default:
                    throw new Exception("Invalid storage type.");
            }

            Database = new DManager(_db);

            Database.SyncPlayers();
            Database.SyncChannels();

            #endregion
        }    
        #endregion
        
        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                PlayerHooks.PlayerPostLogin -= PostLogin;

                if (!postInitialized)
                    ServerApi.Hooks.GamePostInitialize.Deregister(this, PostInitialize);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Constructor
        public SSBot(Main game) : base(game)
        {
            Order = 1000;
        }       
        #endregion

        #region OnInitialize
        private void OnInitialize(EventArgs e)
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("bot.manage", BotEdit, "bot"));

            var configPath = Path.Combine(TShock.SavePath, "ServerBot.json");
            (Config = Config.Read(configPath)).Write(configPath);

            BuiltInCommandRegistration.RegisterCommands();
        }
        #endregion

        #region BotEdit Command
        private void BotEdit(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. Usage: /bot [name] [chat color(255,255,255)]");
                return;
            }

            var name = args.Parameters[0];
            var colorString = args.Parameters.Count >= 2 ? args.Parameters[1] : "255,255,255";
            var colArr = colorString.Split(',');
            byte r;
            byte g;
            byte b;

            if (byte.TryParse(colArr[0], out r) && byte.TryParse(colArr[1], out g)
                && byte.TryParse(colArr[2], out b) && r <= 255 && g <= 255 && b <= 255)
            {
                Bot.name = name;
                Bot.chatColor = colorString;

                args.Player.SendSuccessMessage("Bot makeover complete!");
            }
            else
                args.Player.SendErrorMessage("Invalid color string. Format: 255,255,255");
        }
        #endregion

        #region PostInitialize
        private void PostInitialize(EventArgs e)
        {
            Bot.name = Config.BotName;
            Bot.chatColor = Config.BotChatColor;
            if (ServerApi.Hooks.GamePostInitialize.Deregister(this, PostInitialize))
                postInitialized = true;
        }
        #endregion

        #region OnGreet
        private void OnGreet(GreetPlayerEventArgs e)
        {
            if (!TShock.Config.DisableUUIDLogin)
            {
                if (TShock.Players[e.Who].IsLoggedIn)
                    PostLogin(new PlayerPostLoginEventArgs(TShock.Players[e.Who]));
                else
                {
                    var player = new BPlayer("~^" + TShock.Players[e.Who].Name)
                    {
                        index = e.Who,
                        online = true,
                        Channel = globalChannel
                    };
                    Storage.players.Add(player);
                    TShock.Players[e.Who].SendSuccessMessage("[{0}]: {1}", 
                        player.Channel.name, player.Channel.topic);
                }
            }
            else
            {
                var player = new BPlayer("~^" + TShock.Players[e.Who].Name)
                {
                    index = e.Who,
                    online = true,
                    Channel = globalChannel
                };
                Storage.players.Add(player);
                TShock.Players[e.Who].SendSuccessMessage("[{0}]: {1}",
                    player.Channel.name, player.Channel.topic);
            }
        }
        #endregion

        #region PostLogin
        private void PostLogin(PlayerPostLoginEventArgs e)
        {
            if (Storage.GetPlayerByName(e.Player.UserAccountName) != null)
            {
                var player = Storage.GetPlayerByName(e.Player.UserAccountName);

                player.index = e.Player.Index;
                player.online = true;
                player.Channel = globalChannel;

                e.Player.SendSuccessMessage("[{0}]: {1}",
                    player.Channel.name, player.Channel.topic);

                if (!player.Channel.users.Contains(player.name))
                    player.Channel.users.Add(player.name);
                if (!player.Channel.accessLevels.ContainsKey(player.name))
                    player.Channel.accessLevels.Add(player.name, 0);
            }
            else
            {
                if (Storage.GetPlayerByName("~^" + e.Player.Name) != null)
                {
                    var player = Storage.GetPlayerByName("~^" + e.Player.Name);

                    player.name = e.Player.UserAccountName;
                    player.index = e.Player.Index;
                    player.online = true;
                    player.Channel = globalChannel;

                    Database.InsertPlayer(player);

                    e.Player.SendSuccessMessage("[{0}]: {1}",
                        player.Channel.name, player.Channel.topic);

                    if (!player.Channel.users.Contains(player.name))
                        player.Channel.users.Add(player.name);
                    if (!player.Channel.accessLevels.ContainsKey(player.name))
                        player.Channel.accessLevels.Add(player.name, 0);
                }
                else
                {
                    var player = new BPlayer(e.Player.UserAccountName)
                    {index = e.Player.Index, online = true, Channel = globalChannel};

                    Storage.players.Add(player);

                    Database.InsertPlayer(player);

                    e.Player.SendSuccessMessage("[{0}]: {1}",
                        player.Channel.name, player.Channel.topic);

                    if (!player.Channel.users.Contains(player.name))
                        player.Channel.users.Add(player.name);
                    if (!player.Channel.accessLevels.ContainsKey(player.name))
                        player.Channel.accessLevels.Add(player.name, 0);
                }
            }
        }
        #endregion

        #region OnLeave
        private void OnLeave(LeaveEventArgs e)
        {
            if (TShock.Players[e.Who].IsLoggedIn)
            {
                if (Storage.GetPlayerByName(TShock.Players[e.Who].UserAccountName) == null)
                    return;

                var player = Storage.GetPlayerByName(TShock.Players[e.Who].UserAccountName);
                player.online = false;
                player.Channel = null;
                player.partConfirmed = false;
                player.invitedChannel = string.Empty;
                Database.SavePlayer(player);
                player.index = -1;
            }
            else if (Storage.GetPlayerByName("~^" + TShock.Players[e.Who].Name) != null)
            {
                var player = Storage.GetPlayerByName("~^" + TShock.Players[e.Who].Name);
                player.index = -1;
                player.Channel = null;
                player.partConfirmed = false;
                player.invitedChannel = string.Empty;
                player.online = false;
            }
        }
        #endregion

        #region OnChat
        private void OnChat(ServerChatEventArgs e)
        {
            var player = TShock.Players[e.Who];

            if (player == null)
                return;

            var playerName = player.IsLoggedIn ? player.UserAccountName : player.Name;

            #region TShock Command Handling
            #region /me command
            if (e.Text.StartsWith("/me"))
            {
                e.Handled = true;

                var args = e.Text.Split(' ').ToList();
                args.RemoveAt(0);

                if (args.Count == 0)
                {
                    player.SendErrorMessage("Invalid syntax! Proper syntax: /me <text>");
                    return;
                }
                if (player.mute)
                {
                    player.SendErrorMessage("You are muted.");
                    return;
                }

                foreach (var ply in Storage.players)
                {
                    if (ply != null && ply.online)
                        if (!ply.ignoredPlayers.Contains((playerName).ToLower()))
                        {
                            ply.TSPlayer.SendMessage(string.Format("*{0} {1}", player.Name, 
                                string.Join(" ", args)),
                                205, 133, 63);
                        }
                }

                TSPlayer.Server.SendMessage(
                    String.Format(TShock.Config.ChatFormat, player.Group.Name, player.Group.Prefix,
                        player.Name, player.Group.Suffix, e.Text),
                    player.Group.R, player.Group.G, player.Group.B);

                TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.",
                            player.Name, TShock.Config.CommandSpecifier, e.Text), Color.PaleVioletRed, player);
                return;
            }
            #endregion

            #region /reply command
            if (e.Text.StartsWith("/r ") || e.Text.StartsWith("/reply "))
            {
                if (player.LastWhisper == null)
                    return;

                if (Storage.GetPlayerByName(player.LastWhisper.IsLoggedIn
                    ? player.LastWhisper.UserAccountName
                    : player.LastWhisper.Name).ignoredPlayers.Contains(
                        playerName))
                {
                    Bot.PrivateSay("Message not sent: You have been ignored.", player);
                    TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.", player.Name,
                        TShock.Config.CommandSpecifier, e.Text), Color.PaleVioletRed, player);
                    e.Handled = true;
                    return;
                }
            }
            #endregion

            #region /whisper command
            if (e.Text.StartsWith("/w ") || e.Text.StartsWith("/whisper "))
            {
                e.Handled = true;

                var args = e.Text.Split(' ').ToList();
                args.RemoveAt(0);

                if (args.Count < 2)
                {
                    player.SendErrorMessage("Invalid syntax! Proper syntax: /whisper <player> <text>");
                    return;
                }
                
                var players = TShock.Utils.FindPlayer(args[0]);

                if (players.Count == 0)
                    player.SendErrorMessage("Invalid player!");

                else if (players.Count > 1)
                    TShock.Utils.SendMultipleMatchError(player, players.Select(p => p.Name));

                else if (player.mute)
                    player.SendErrorMessage("You are muted.");

                else
                {
                    var plr = players[0];
                    var plrName = plr.IsLoggedIn ? plr.UserAccountName : plr.Name;

                    if (Storage.GetPlayerByName(plrName).ignoredPlayers.Contains(playerName))
                    {
                        Bot.PrivateSay("Message not sent: You have been ignored", player);
                        TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.",
                            player.Name, TShock.Config.CommandSpecifier, e.Text), Color.PaleVioletRed, player);
                        e.Handled = true;
                        return;
                    }

                    var msg = string.Join(" ", args.ToArray(), 1, args.Count - 1);
                    plr.SendMessage(String.Format("<From {0}> {1}", player.Name, msg), Color.MediumPurple);
                    player.SendMessage(String.Format("<To {0}> {1}", plr.Name, msg), Color.MediumPurple);
                    plr.LastWhisper = player;
                    player.LastWhisper = plr;
                    TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.",
                        player.Name, TShock.Config.CommandSpecifier, e.Text), Color.PaleVioletRed, player);
                }
            }
            #endregion
            #endregion

            if (e.Text.StartsWith("/"))
                return;

            e.Handled = true;

            #region Handle ignores and private commands
            if (!e.Text.StartsWith(Config.PrivateCharacter.ToString(CultureInfo.InvariantCulture)))
            {
                foreach (var ply in Storage.players)
                {
                    if (ply != null && ply.online)
                        if (!ply.ignoredPlayers.Contains((player.IsLoggedIn
                            ? player.UserAccountName
                            : player.Name).ToLower()))
                        {
                            ply.TSPlayer.SendMessage(
                                String.Format(TShock.Config.ChatFormat, player.Group.Name, player.Group.Prefix,
                                    player.Name, player.Group.Suffix, e.Text),
                                player.Group.R, player.Group.G, player.Group.B);
                        }
                }

                TSPlayer.Server.SendMessage(
                    String.Format(TShock.Config.ChatFormat, player.Group.Name, player.Group.Prefix,
                        player.Name, player.Group.Suffix, e.Text),
                    player.Group.R, player.Group.G, player.Group.B);

                Log.Info(string.Format("Broadcast: {0}", e.Text));
            }
            else
            {
                TSPlayer.Server.SendMessage(
                    String.Format(TShock.Config.ChatFormat, player.Group.Name, player.Group.Prefix,
                        player.Name, player.Group.Suffix, e.Text),
                    player.Group.R, player.Group.G, player.Group.B);

                player.SendMessage(
                    String.Format(TShock.Config.ChatFormat, player.Group.Name, player.Group.Prefix,
                        player.Name, player.Group.Suffix, e.Text),
                    player.Group.R, player.Group.G, player.Group.B);
                chatHandler.HandleChat(e.Text, Storage.GetPlayerByName(playerName));
            }
            #endregion

            if (!e.Text.StartsWith(Config.CommandCharacter.ToString(CultureInfo.InvariantCulture)))
                return;

            if (!player.IsLoggedIn)
                Bot.PrivateSay("A registered account is required to use the bot", player);

            chatHandler.HandleChat(e.Text, Storage.GetPlayerByName(playerName));
        }
        #endregion
    }
}