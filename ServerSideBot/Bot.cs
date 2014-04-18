using System;
using System.Collections.Generic;
using TShockAPI;

namespace ServerSideBot
{
    public class Bot
    {
        public string name;
        public string chatColor;

        /*                msg     reply             */
        public Dictionary<string, string> Replies
        {
            get { return SSBot.Database.GetAutomatedReplies(); }
        }

        private Color Color
        {
            get
            {
                var b = new Byte[3];
                var rgb = chatColor.Split(',');
                b[0] = byte.Parse(rgb[0]);
                b[1] = byte.Parse(rgb[1]);
                b[2] = byte.Parse(rgb[2]);
                return new Color(b[0], b[1], b[2]);
            }
        }

        public Bot()
        {
            name = SSBot.Config.BotName;
            chatColor = SSBot.Config.BotChatColor;
        }

        /// <summary>
        /// Global message to all players from the bot
        /// </summary>
        /// <param name="text">Text to be sent</param>
        public void Say(string text)
        {
            TSPlayer.All.SendMessage(string.Format("<{0}>: {1}", name, text), Color);
            TSPlayer.Server.SendMessage(string.Format("<{0}>: {1}", name, text), Color);
            Log.Info(string.Format("Botcast: {0}", text));
        }
        public void Say(string text, params object[] args)
        {
            TSPlayer.All.SendMessage(string.Format("<{0}>: {1}", name,
                string.Format(text, args)), Color);
            TSPlayer.Server.SendMessage(string.Format("<{0}>: {1}", name,
                string.Format(text, args)), Color);
            Log.Info(string.Format("Botcast: {0}", text));
        }

        /// <summary>
        /// Emulates a whisper sent from the bot
        /// </summary>
        /// <param name="text">Text to be sent</param>
        /// <param name="player">Player to send text to</param>
        public void PrivateSay(string text, TSPlayer player)
        {
            player.SendMessage(string.Format("<From {0}> {1}", name, text), Color.MediumPurple);
        }
        /// <summary>
        /// Emulates a whisper sent from the bot
        /// </summary>
        /// <param name="text">Text to be sent</param>
        /// <param name="player">Player to send text to</param>
        /// <param name="args">arguments for string.Format</param>
        public void PrivateSay(string text, TSPlayer player, params object[] args)
        {
            player.SendMessage(string.Format("<From {0}> {1}", name,
                string.Format(text, args)), Color.MediumPurple);
        }
    }
}
