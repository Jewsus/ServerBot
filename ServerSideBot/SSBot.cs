using System;
using System.Globalization;
using System.IO;
using System.Data;
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
        public static Storage Storage;
        public static Config Config = new Config();
        public static Bot Bot = new Bot();
        public static ChatHandler ChatHandler = new ChatHandler();

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


        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerChat.Register(this, OnChat, 1);
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            PlayerHooks.PlayerPostLogin += PostLogin;

            Storage = new Storage();

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

            #endregion
        }

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

        public SSBot(Main game) : base(game)
        {
            Order = 1000;
        }

        private void OnInitialize(EventArgs e)
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("bot.manage", JoinCmd, "bot"));

            var configPath = Path.Combine(TShock.SavePath, "ServerBot.json");
            (Config = Config.Read(configPath)).Write(configPath);

            BuiltInCommandRegistration.RegisterCommands();
        }

        private void JoinCmd(CommandArgs args)
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

        private void PostInitialize(EventArgs e)
        {

            if (ServerApi.Hooks.GamePostInitialize.Deregister(this, PostInitialize))
                postInitialized = true;
        }

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
                        online = true
                    };
                    Storage.players.Add(player);
                }
            }
            else
            {
                var player = new BPlayer("~^" + TShock.Players[e.Who].Name)
                {
                    index = e.Who,
                    online = true
                };
                Storage.players.Add(player);
            }
        }

        private void PostLogin(PlayerPostLoginEventArgs e)
        {
            if (Storage.GetPlayerByName(e.Player.UserAccountName) != null)
            {
                var player = Storage.GetPlayerByName(e.Player.UserAccountName);

                player.index = e.Player.Index;
                player.online = true;
            }
            else
            {
                if (Storage.GetPlayerByName("~^" + e.Player.Name) != null)
                {
                    var player = Storage.GetPlayerByName("~^" + e.Player.Name);

                    player.name = e.Player.UserAccountName;
                    player.index = e.Player.Index;
                    player.online = true;

                    Database.InsertPlayer(player);
                }
                else
                {
                    var player = new BPlayer(e.Player.UserAccountName)
                    {index = e.Player.Index, online = true};

                    Storage.players.Add(player);

                    Database.InsertPlayer(player);
                }
            }
        }

        private void OnLeave(LeaveEventArgs e)
        {
            if (TShock.Players[e.Who].IsLoggedIn)
            {
                if (Storage.GetPlayerByName(TShock.Players[e.Who].UserAccountName) == null)
                    return;

                var player = Storage.GetPlayerByName(TShock.Players[e.Who].UserAccountName);
                player.online = false;
                Database.SavePlayer(player);
                player.index = -1;
            }
            else if (Storage.GetPlayerByName("~^" + TShock.Players[e.Who].Name) != null)
            {
                var player = Storage.GetPlayerByName("~^" + TShock.Players[e.Who].Name);
                player.index = -1;
                player.online = false;
            }
        }

        private void OnChat(ServerChatEventArgs e)
        {
            var player = TShock.Players[e.Who];

            if (player == null)
                return;

            if (e.Text.StartsWith("/"))
                return;

            e.Handled = true;

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

                if (!ChatHandler.HandleChat(e.Text, Storage.GetPlayerByName(player.UserAccountName)))
                    Bot.PrivateSay("Command failed.", player);
            }

            if (!e.Text.StartsWith(Config.CommandCharacter.ToString(CultureInfo.InvariantCulture)))
                return;

            if (!player.IsLoggedIn)
                Bot.PrivateSay("A registered account is required to use the bot", player);

            if (!ChatHandler.HandleChat(e.Text, Storage.GetPlayerByName(player.UserAccountName)))
            {
                Bot.PrivateSay("Command failed.", player);
            }
        }
    }
}
