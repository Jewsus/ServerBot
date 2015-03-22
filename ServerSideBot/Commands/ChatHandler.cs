using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ServerSideBot.Extensions;
using TShockAPI;

namespace ServerSideBot.Commands
{
    public class ChatHandler
    {
        internal readonly List<BotCommand> commands = new List<BotCommand>();

	    public ChatHandler()
	    {
		    RegisterCommand("", BuiltInCommands.Say, "say");
	    }

	    public bool HandleChat(string text, TSPlayer player, bool queue = false)
	    {
		    if (text.Length < 2)
		    {
			    return false;
		    }

		    var priv = text[0] == SSBot.config.PrivateCharacter;

		    var newText = text.Remove(0, 1);

		    var split = newText.Split(' ');
		    var parms = new List<string>();

		    parms.AddRange(split);
		    var name = parms[0];

		    parms.RemoveAt(0);

		    var args = new BotCommandArgs(name, parms, SSBot.bot, player.GetBotUser()) {priv = priv};
		    var cmd = commands.FirstOrDefault(c => c.Names.Contains(name));

		    if (cmd == null)
		    {
			    SSBot.bot.PrivateSay("Invalid command", player);
			    return false;
		    }

		    var hasPermission = cmd.Permissions.Any(s => player.Group.HasPermission(s));
		    if (!hasPermission)
		    {
			    SSBot.bot.PrivateSay("Invalid permission level", player);

			    TShock.Utils.SendLogs(string.Format("{0} tried to execute: {1}{2}.",
				    player.Name,
				    priv
					    ? SSBot.config.PrivateCharacter
					    : SSBot.config.CommandCharacter,
				    name),
				    Color.PaleVioletRed, player);
			    return false;
		    }

			//This stops the bot replying before the command is displayed.
		    if (queue)
		    {
			    ThreadPool.QueueUserWorkItem(CommandQueue, new object[] {cmd, args});
			    return false;
		    }

		    TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.",
			    player.Name,
			    priv
				    ? SSBot.config.PrivateCharacter
				    : SSBot.config.CommandCharacter, name + " " + string.Join(" ", parms)),
			    Color.PaleVioletRed, player);
		    cmd.Delegate(args);

		    return args.handled;
	    }

		private void CommandQueue(object obj)
		{
			Thread.Sleep(300);
			var array = obj as object[];
			var cmd = array[0] as BotCommand;
			var args = array[1] as BotCommandArgs;

			cmd.Delegate(args);
		}

	    public void RegisterCommand(string permission, CommandDelegate command, params string[] names)
        {
            commands.Add(new BotCommand(permission, command, names));
        }

        public void RegisterCommand(List<string> permissions, CommandDelegate command, params string[] names)
        {
            commands.Add(new BotCommand(permissions, command, names));
        }
    }

    public delegate void CommandDelegate(BotCommandArgs args);

    public class BotCommandArgs : EventArgs
    {
        private string Command;
        public List<string> Parameters;
        public BotUser Player;
        public readonly Bot Bot;
        public bool priv;
	    public bool handled;

        public BotCommandArgs(string command, List<string> parms, Bot bot, BotUser ply)
        {
            Command = command;
            Parameters = parms;
            Player = ply;
            Bot = bot;
        }
    }

    public class BotCommand
    {
        public List<string> Names = new List<string>();
        public List<string> Permissions = new List<string>(); 
		public CommandDelegate Delegate;

        public BotCommand(string permission, CommandDelegate com, params string[] names)
        {
            Names = names.ToList();
            Delegate = com;
            Permissions = new List<string> {permission};
        }

        public BotCommand(List<string> permissions, CommandDelegate com, params string[] names)
		{
			Names = names.ToList();
            Permissions = permissions;
			Delegate = com;
		}
    }
}