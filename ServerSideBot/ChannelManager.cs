using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace ServerSideBot
{
    public class ChannelManager
    {
        public List<Channel> Channels = new List<Channel>();

        public void CreateChannel(string name, BPlayer player)
        {
            foreach (var c in Channels)
                if (c.name == name)
                {
                    SSBot.Bot.PrivateSay("Channel creation failed: Channel of same name exists.", 
                        player.TSPlayer);
                    return;
                }

            var chan = new Channel
            {
                name = name,
                owner = player.name,
                accessLevels = new Dictionary<string, int> { { player.name, 5 } }
            };
            Channels.AddEx(chan);
            chan.AttemptJoin(player);
        }

        public List<Channel> GetChannelsByName(string name)
        {
            var retList = new List<Channel>();
            name = name.ToLower();

            foreach (var c in Channels)
            {
                if (c.name.ToLower() == name)
                    return new List<Channel> {c};

                if (c.name.ToLower().Contains(name))
                    retList.Add(c);
            }
            return retList;
        }
    }

    /// <summary>
    /// Represents a chat channel that can be joined and modified
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Name of the channel
        /// </summary>
        public string name;
        /// <summary>
        /// Owner of the channel
        /// </summary>
        public string owner;
        /// <summary>
        /// Channel modes enforced in the channel
        /// </summary>
        public string modes;
        /// <summary>
        /// Access levels of players in the channel
        /// </summary>
        public Dictionary<string, int> accessLevels;
        /// <summary>
        /// Banned users of the channel
        /// </summary>
        public List<string> banList;
        /// <summary>
        /// Connected users of the channel
        /// </summary>
        public List<string> users;
        /// <summary>
        /// Password of the channel
        /// </summary>
        public string password;
        /// <summary>
        /// User limit of the channel
        /// </summary>
        public int capacity = -1;
        /// <summary>
        /// Topic of the channel
        /// </summary>
        public string topic;

        /// <summary>
        /// Initializes a new instance of the Channel class with default values
        /// </summary>
        public Channel()
        {
            modes = string.Empty;
            name = string.Empty;
            owner = string.Empty;
            banList = new List<string>();
            accessLevels = new Dictionary<string, int>();
            users = new List<string>();
            password = string.Empty;
            topic = string.Empty;
        }

        /// <summary>
        /// Sends a message to all players in a channel
        /// </summary>
        /// <param name="message">Message to be sent</param>
        public void SendMessage(string message)
        {
            foreach (var ply in users)
            {
                var player = TShock.Utils.FindPlayer(ply)[0];
                if (player != null && player.ConnectionAlive && player.Active)
                {
                    player.SendWarningMessage("[{0}]: {1}", name, message);
                }
            }
        }

        /// <summary>
        /// Sends a message to all players in a channel
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="args">Format arguments in the message</param>
        public void SendMessage(string message, params object[] args)
        {
            foreach (var ply in users)
            {
                var player = TShock.Utils.FindPlayer(ply)[0];
                if (player != null && player.ConnectionAlive && player.Active)
                {
                    player.SendWarningMessage("[{0}]: {1}", name, string.Format(message, args));
                }
            }
        }

        /// <summary>
        /// Attempts to join a player to a channel
        /// </summary>
        /// <param name="player">player joining the channel</param>
        /// <param name="password">password sent</param>
        public void AttemptJoin(BPlayer player, string password = "")
        {
            if (owner == player.name)
            {
                if (player.Channel != SSBot.globalChannel)
                    player.Channel.users.Remove(player.name);

                player.Channel = this;
                users.Add(player.name);
                SSBot.Bot.PrivateSay("Now talking in " + name, player.TSPlayer);
                return;
            }

            if (banList.Contains(player.name))
            {
                SSBot.Bot.PrivateSay("You have been banned from this channel", player.TSPlayer);
                return;
            }

            if (accessLevels.ContainsKey(player.name))
            {
                if (accessLevels[player.name] < 1 && modes.Contains("i"))
                {
                    SSBot.Bot.PrivateSay("You don't have access to this channel", player.TSPlayer);
                    return;
                }
            }
            else
            {
                if (modes.Contains("i"))
                {
                    SSBot.Bot.PrivateSay("You don't have access to this channel", player.TSPlayer);
                    return;
                }
            }

            if (modes.Contains("k") && password != this.password)
            {
                SSBot.Bot.PrivateSay("Incorrect password", player.TSPlayer);
                return;
            }

            if (capacity != -1 && users.Count > capacity)
            {
                SSBot.Bot.PrivateSay("Channel is full", player.TSPlayer);
                return;
            }

            if (modes.Contains("r") && player.TSPlayer.IsLoggedIn == false)
            {
                SSBot.Bot.PrivateSay("You must be logged in to join this channel", player.TSPlayer);
                return;
            }


            if (player.Channel != SSBot.globalChannel)
                player.Channel.users.Remove(player.name);

            SSBot.Bot.PrivateSay("Now talking in #" + name, player.TSPlayer);
            SendMessage("{0} has joined", player.name);
            player.Channel = this;
            users.Add(player.name);
        }

        public void Part(BPlayer player)
        {
            users.Remove(player.name);
            SSBot.globalChannel.users.Add(player.name);
        }

        public void Kick(BPlayer kicker, BPlayer kickee, string reason = "global channel kick")
        {
            users.Remove(kickee.name);
            if (kickee.Channel == SSBot.globalChannel)
                kickee.TSPlayer.Disconnect("Kicked: " + reason);
            else
            {
                SSBot.Bot.PrivateSay("Kicked from {0} ({1})", kickee.TSPlayer,
                    kickee.Channel, reason);
                kickee.Channel = SSBot.globalChannel;

                SendMessage("{0} kicked {1} ({2})", kicker.name, kickee,name, reason);
            }
        }
    }

    public class GlobalChannel : Channel
    {
        public GlobalChannel()
        {
            name = "Global";
            topic = "Welcome to " + (TShock.Config.UseServerName
                ? TShock.Config.ServerName
                : Main.worldName);
        }
    }
}
