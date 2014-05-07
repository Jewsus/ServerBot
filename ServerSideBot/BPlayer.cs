using System;
using System.Collections.Generic;
using TShockAPI;

namespace ServerSideBot
{
    public class BPlayer
    {
        public int index;
        public bool online;
        public bool partConfirmed;
        public string name;
        public TSPlayer TSPlayer { get { return TShock.Players[index]; } }
        public List<string> ignoredPlayers = new List<string>();
        public Channel Channel { get; set; }
        public string invitedChannel = string.Empty;
        public List<string> links = new List<string>();
        public DateTime lastQuery;

        public BPlayer(string name)
        {
            this.name = name;
        }
    }
}
