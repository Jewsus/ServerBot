using System;
using TShockAPI;
using TShockAPI.DB;

namespace ServerBot
{
	/// <summary>
	/// Bot commands that come with the bot
	/// </summary>
	public class BuiltinBotCommands
	{
		#region BotHelp
		public static void BotHelp(BotCommandArgs args)
		{
            if (args.Parameters.Count == 1)
            {
                switch (args.Parameters[0])
                {
                    case "register":
                        args.Bot.Say("To register, use /register <password>");
                        args.Bot.Say("<password> can be anything, and you define it personally.");
                        args.Bot.Say("Always remember to keep your password secure!");
                        bTools.LogToConsole(ConsoleColor.Cyan, "{0} used {1} to execute: help register", args.Player.Name, bTools.Bot.Name);
                        break;
                    case "item":
                        args.Bot.Say("To spawn items, use the command /item");
                        args.Bot.Say("Items that are made of multiple words MUST be wrapped in quotes");
                        args.Bot.Say("Eg: /item \"hallowed repeater\"");
                        bTools.LogToConsole(ConsoleColor.Cyan, "{0} used {1} to execute: help item", args.Player.Name, bTools.Bot.Name);
                        break;
                }
            }
            else
            {
                args.Bot.Say("Whoops! Try using ^ help item or ^ help register");
            }
		}
		#endregion
		
		#region BotKill
		public static void BotKill(BotCommandArgs args)
		{
		    if (args.Parameters.Count <= 0)
                return;

		    var targets = TShock.Utils.FindPlayer(args.Parameters[0]);
		    if (targets.Count < 1)
		    {
		        TShock.Utils.SendMultipleMatchError(args.Player, targets);
		        return;
		    }
		    var target = targets[0];
				
		    if (args.Player.Group.HasPermission("kill"))
		    {
		        target.DamagePlayer(target.TPlayer.statLifeMax * target.TPlayer.statDefense);
		        target.TPlayer.dead = true;
		        target.Dead = true;
		        args.Bot.Say("{1} just had me kill {2}!", args.Player.Name, target.Name);
		        bTools.LogToConsole(ConsoleColor.Cyan, "{0} used {1} to execute: kill on {2}", args.Player.Name, bTools.Bot.Name, target.Name);
		    }
		    else
		    {
		        args.Bot.Private(args.Player, "Sorry, but you don't have the permission to use kill.");
		        bTools.LogToConsole(ConsoleColor.Cyan, "{0} failed to use kill on {2} because of lack of permissions.", args.Player.Name, target.Name);
		    }
		}
		#endregion
		
		#region BotBan
		public static void BotBan(BotCommandArgs args)
		{
            var targets = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (targets.Count < 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, targets);
                return;
            }
            var target = targets[0];
			
			if (args.Player.Group.HasPermission("ban"))
			{
				TShock.Utils.Ban(target, bTools.Bot.Name + " ban");
				bTools.LogToConsole(ConsoleColor.Cyan, "{0} used {1} to execute: ban on {2}", args.Player.Name, args.Bot.Name, target.Name);
			}
			else
			{
				args.Bot.Private(args.Player, "Sorry, but you don't have permission to use ban.");
				bTools.LogToConsole(ConsoleColor.Cyan, "{0} failed to use \"ban\" on {1} because of a lack of permission.", args.Player.Name, target.Name);
			}
		}
		#endregion
		
		#region BotKick
		public static void BotKick(BotCommandArgs args)
		{
            var targets = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (targets.Count < 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, targets);
                return;
            }
            var plr = targets[0];

			if (args.Player.Group.HasPermission("kick"))
			{
                TShock.Utils.Kick(plr, args.Bot.Name + " forcekick");
                bTools.LogToConsole(ConsoleColor.Cyan, "{0} used {1} to execute: kick on {2}", args.Player.Name, args.Bot.Name, plr.Name);
			}
			else
            {
				args.Bot.Private(args.Player, "Sorry, but you don't have permission to use kick.");
				bTools.LogToConsole(ConsoleColor.Cyan, "{0} failed to use kick on {1} because of a lack of permission.", args.Player.Name, plr.Name);
            }
		}
		#endregion
		
		#region BotMute
		public static void BotMute(BotCommandArgs args)
		{
            var targets = TShock.Utils.FindPlayer(args.Parameters[0]);
            if (targets.Count < 1)
            {
                TShock.Utils.SendMultipleMatchError(args.Player, targets);
                return;
            }
            var plr = targets[0];

			if (args.Player.Group.HasPermission("mute"))
			{
				plr.mute = !plr.mute;
			    args.Bot.Say("{0} was {1}muted!", plr.Name, plr.mute ? "" : "un");
				bTools.LogToConsole(ConsoleColor.Cyan, "{0} used {1} to execute: mute on {2}", args.Player.Name, args.Bot.Name, plr.Name);
			}
			else
		    {
				bTools.LogToConsole(ConsoleColor.Cyan, "{0} failed to use mute on {1} because of a lack of permission.", 
                    args.Player.Name, plr.Name);
               	args.Bot.Private(args.Player, "Sorry, but you don't have permission to use mute.");
		    }
		}
		#endregion
		
		#region BotButcher
		public static void BotButcher(BotCommandArgs args)
		{
			if (args.Player.Group.HasPermission("butcher"))
			{
				Commands.HandleCommand(args.Player, "/butcher");
				args.Bot.Say("I butchered all hostile NPCs!");
				bTools.LogToConsole(ConsoleColor.Cyan,"{0} used {1} to execute: butcher", args.Player.Name, args.Bot.Name);
			}
			else
			{
				var r = new Random();
                var p = r.Next(1, 100);
                if (p <= bTools.botConfig.command_Success_Percent)
                {
                    Commands.HandleCommand(TSPlayer.Server, "/butcher");
                   	args.Bot.Say("I butchered all hostile NPCs!");
					bTools.LogToConsole(ConsoleColor.Cyan,"{0} used {1} to execute: butcher", args.Player.Name, args.Bot.Name);
                }
                else
                {
                	args.Bot.Say("Sorry {0}, you rolled a {1}. You need to roll less than {2} to butcher", 
                        args.Player.Name, p, bTools.botConfig.command_Success_Percent);
                }
			}
		}
		#endregion
		
		#region BotTriviaStart
		public static void BotTriviaStart(BotCommandArgs args)
		{
		    if (args.Parameters.Count > 0)
		    {
		        int numq;
		        if (!int.TryParse(args.Parameters[0], out numq))
		        {
		            args.Bot.Say("You didn't provide a valid number of questions for the game.");
		            return;
		        }
		        args.Bot.Trivia.StartGame(numq);
		    }
		    else
		        args.Bot.Say("Proper format of \"^ starttrivia\": ^ starttrivia <number of questions to ask>");
		}
		#endregion
		
		#region BotTriviaAnswer
		public static void BotTriviaAnswer(BotCommandArgs args)
		{
			if (args.Bot.Trivia.OngoingGame)
			{
				args.Bot.Trivia.CheckAnswer(string.Join(" ", args.Parameters), args.Player.Name);
			}
		}
		#endregion

        #region BotBadwords
        public static void BotBadWords(BotCommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                switch (args.Parameters[0])
                {
                    case "add":
                        using (var reader = bTools.db.QueryReader("SELECT * FROM BotSwear WHERE SwearBlock = @0", args.Parameters[1]))
                        {
                            if (!reader.Read())
                            {
                                bTools.db.Query("INSERT INTO BotSwear (SwearBlock) VALUES (@0)", args.Parameters[1]);
                                args.Player.SendMessage(string.Format("Added {0} into the banned word list.", args.Parameters[1]), Color.CadetBlue);
                                bTools.Swearwords.Add(args.Parameters[1].ToLower());
                            }
                            else
                            {
                                args.Player.SendWarningMessage(string.Format("{0} already exists in the swear list.", args.Parameters[1]));
                            }
                        }
                        break;
                    case "del":
                        using (var reader = bTools.db.QueryReader("SELECT * FROM BotSwear WHERE SwearBlock = @0", args.Parameters[1]))
                        {
                            if (reader.Read())
                            {
                                bTools.db.Query("DELETE FROM BotSwear WHERE SwearBlock = @0", args.Parameters[1]);
                                args.Player.SendMessage(string.Format("Deleted {0} from the banned word list.", args.Parameters[1]), Color.CadetBlue);
                                bTools.Swearwords.Remove(args.Parameters[1].ToLower());
                            }
                            else
                            {
                                args.Player.SendWarningMessage(string.Format("{0} does not exist in the swear list.", args.Parameters[1]));
                            }
                        }
                        break;
                }
            }
            else
            {
                args.Bot.Say("You didn't have a valid number of parameters; Use ^ badwords [add/del] \"word\"");
            }
        }
        #endregion

        #region BotReloadCfg
        public static void BotReloadCfg(BotCommandArgs args)
        {
            bTools.SetUpConfig();

            bTools.Bot.Trivia.LoadConfig(bTools.triviaSavePath);

            args.Player.SendWarningMessage("Reloaded Bot config");
        }
        #endregion

        #region BotPlayerManagement
        public static void KickPlayers(BotCommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendWarningMessage("You didn't have a valid number of parameters; Use ^ player [add/del] \"playername\"");
            }
            else
            {
                if (args.Parameters[0] == "add")
                {
                    using (var reader = bTools.db.QueryReader("SELECT * FROM BotKick WHERE KickNames = @0", args.Parameters[1]))
                    {
                        if (!reader.Read())
                        {
                            bTools.db.Query("INSERT INTO BotKick (KickNames) VALUES (@0)", args.Parameters[1]);
                            args.Player.SendMessage(string.Format("Added {0} to the playermanager list.", args.Parameters[1]), Color.CadetBlue);
                        }
                        else
                        {
                            args.Player.SendWarningMessage(string.Format("{0} already exists in the playermanager list!", args.Parameters[1]));
                        }
                    }
                }
                else if (args.Parameters[0] == "del")
                {
                    using (var reader = bTools.db.QueryReader("SELECT * FROM BotKick WHERE KickNames = @0", args.Parameters[1]))
                    {
                        if (reader.Read())
                        {
                            bTools.db.Query("DELETE FROM BotKick WHERE KickNames = @0", args.Parameters[1]);
                            args.Player.SendMessage(string.Format("Deleted {0} from the playermanager list!", args.Parameters[1]), Color.CadetBlue);
                        }
                        else
                        {
                            args.Player.SendWarningMessage(string.Format("{0} does not exist on the playermanager list!", args.Parameters[1]));
                        }
                    }
                }
            }
        }
        #endregion
    }	
}
