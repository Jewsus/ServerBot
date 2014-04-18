using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServerBot
{
    public class cBotCommand
    {
        /// <summary>
        /// Name of the chat-command to send to the bot
        /// </summary>
        public string CommandName;

        /// <summary>
        /// The message the bot will send to the player, or to the server
        /// </summary>
        public string ReturnMessage;

        /// <summary>
        /// Which commands from TShock.ChatCommands should be used when the bot command is used
        /// </summary>
        public List<string> CommandActions;

        /// <summary>
        /// Whether the bot should broadcast to all players, or just the player who executed the command
        /// </summary>
        public bool noisyCommand;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cn">CommandName</param>
        /// <param name="rm">ReturnMessage</param>
        /// <param name="ca">CommandActions</param>
        /// <param name="noisy">NoisyCommand</param>
        public cBotCommand(string cn, string rm, List<string> ca, bool noisy)
        {
            CommandName = cn;
            ReturnMessage = rm;
            CommandActions = ca;
            noisyCommand = noisy;
        }
    }


    public class CmdConfig
    {
        public List<cBotCommand> customCommands = new List<cBotCommand>();

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static CmdConfig Read(string path)
        {
            if (!File.Exists(path))
                return new CmdConfig();
            return JsonConvert.DeserializeObject<CmdConfig>(File.ReadAllText(path));
        }
    }
}
