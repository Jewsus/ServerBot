using System;
using System.IO;
using System.Reflection;
using System.Timers;
using ServerSideBot.Commands;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ServerSideBot
{
    [ApiVersion(1, 17)]
    public class SSBot : TerrariaPlugin
    {
	    internal static Bot bot;
		internal static Config config = new Config();
		internal static readonly ChatHandler ChatHandler = new ChatHandler();
	    internal static readonly Random R = new Random();

	    private Timer _t = new Timer(1000*300);

        public override string Author
        {
            get { return "White & Ijwu"; }
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
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

		public SSBot(Main game)
			: base(game)
		{
			Order = 1;
		}

		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, GameOnInitialize);
			ServerApi.Hooks.ServerChat.Register(this, PlayerOnChat);
			ServerApi.Hooks.GamePostInitialize.Register(this, GamePostInitialize);
			ServerApi.Hooks.NetGreetPlayer.Register(this, PlayerOnGreet);
			ServerApi.Hooks.GameUpdate.Register(this, GameOnUpdate);
		}

	    protected override void Dispose(bool disposing)
	    {
		    if (disposing)
		    {
				ServerApi.Hooks.GameInitialize.Deregister(this, GameOnInitialize);
				ServerApi.Hooks.ServerChat.Deregister(this, PlayerOnChat);
				ServerApi.Hooks.GamePostInitialize.Deregister(this, GamePostInitialize);
				ServerApi.Hooks.NetGreetPlayer.Deregister(this, PlayerOnGreet);
				ServerApi.Hooks.GameUpdate.Deregister(this, GameOnUpdate);
		    }
		    base.Dispose(disposing);
	    }

	    private void GameOnInitialize(EventArgs args)
		{
			var path = Path.Combine(TShock.SavePath, "ServerBot.json");
			(config = Config.Read(path)).Write(path);
		}

		private void GamePostInitialize(EventArgs args)
		{
			bot = new Bot(config.BotName);
			bot.SetChatColor(config.BotChatColor);
			bot.SendPlayerInfo();
			bot.SendPlayerActive();
			bot.SendPlayerUpdate();
			_t.Elapsed += OnBotUpdateRequired;
			_t.Start();
		}

	    private void PlayerOnChat(ServerChatEventArgs args)
		{
			var player = TShock.Players[args.Who];

			if (player == null)
			{
				return;
			}

		    if (args.Text.Length > 2)
		    {
			    if (args.Text[0] == config.PrivateCharacter)
			    {
				    args.Handled = ChatHandler.HandleChat(args.Text, player);
			    }
			    else if (args.Text[0] == config.CommandCharacter)
				{
					ChatHandler.HandleChat(args.Text, player, true);
			    }
		    }
	    }

		private void PlayerOnGreet(GreetPlayerEventArgs args)
		{
			bot.SendPlayerInfo();
			bot.SendPlayerActive();
			bot.SendPlayerUpdate();
		}

		private void OnBotUpdateRequired(object sender, ElapsedEventArgs e)
		{
			bot.SendPlayerUpdate();
		}

		private void GameOnUpdate(EventArgs args)
		{
		}
    }
}