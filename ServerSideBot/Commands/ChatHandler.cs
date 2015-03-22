using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TShockAPI;

namespace ServerSideBot.Commands
{
    public class ChatHandler
    {
        public readonly List<_Command> Commands = new List<_Command>(); 

        public bool HandleChat(string text, BotUser player)
        {
			//bool _private = 
			//	text.StartsWith(SSBot.Config.PrivateCharacter.ToString(CultureInfo.InvariantCulture));

			//var newText = text.Remove(0, 1);

			//var split = newText.Split(' ');
			//var parms = new List<string>();

			//parms.AddRange(split);
			//var name = parms[0];

			//parms.RemoveAt(0);

			//var args = new _CommandArgs(name, parms, SSBot.Bot, player) {_private = _private};
			//var cmd = Commands.FirstOrDefault(c => c.Names.Contains(name));

			//if (cmd == null)
			//{
			//	SSBot.Bot.PrivateSay("Invalid command", player.TSPlayer);
			//	return false;
			//}

			//var hasPermission = cmd.Permissions.Any(s => player.TSPlayer.Group.HasPermission(s));
			//if (!hasPermission)
			//{
			//	SSBot.Bot.PrivateSay("Invalid permission level", player.TSPlayer);

			//	TShock.Utils.SendLogs(string.Format("{0} tried to execute: {1}{2}.",
			//		player.name,
			//		_private
			//			? SSBot.Config.PrivateCharacter
			//			: SSBot.Config.CommandCharacter, name),
			//		Color.PaleVioletRed, player.TSPlayer);
			//	return false;
			//}


			//TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.",
			//		player.name,
			//		_private
			//			? SSBot.Config.PrivateCharacter
			//			: SSBot.Config.CommandCharacter, name + " " + string.Join(" ", parms)),
			//		Color.PaleVioletRed, args.Player.TSPlayer);
			//cmd.Delegate(args);
			//return true;
	        return false;
        }

        public void RegisterCommand(string permission, CommandDelegate command, params string[] names)
        {
            Commands.Add(new _Command(permission, command, names));
        }

        public void RegisterCommand(List<string> permissions, CommandDelegate command, params string[] names)
        {
            Commands.Add(new _Command(permissions, command, names));
        }
    }

    public delegate bool CommandDelegate(_CommandArgs args);

    public class _CommandArgs : EventArgs
    {
        private string Command;
        public List<string> Parameters;
        public BotUser Player;
        public readonly Bot Bot;
        public bool _private;

        public _CommandArgs(string command, List<string> parms, Bot bot, BotUser ply)
        {
            Command = command;
            Parameters = parms;
            Player = ply;
            Bot = bot;
        }
    }

    public class _Command
    {
        public List<string> Names = new List<string>();
        public List<string> Permissions = new List<string>(); 
		public CommandDelegate Delegate;

        public _Command(string permission, CommandDelegate com, params string[] names)
        {
            Names = names.ToList();
            Delegate = com;
            Permissions = new List<string> {permission};
        }

        public _Command(List<string> permissions, CommandDelegate com, params string[] names)
		{
			Names = names.ToList();
            Permissions = permissions;
			Delegate = com;
		}
    }
}
