using System.Collections.Generic;
using System.Linq.Expressions;
using TShockAPI;

namespace ServerSideBot
{
    public class BPlayer
    {
        public int index;
        public bool online;
        public string name;
        public TSPlayer TSPlayer { get { return TShock.Players[index]; } }
        public List<string> ignoredPlayers = new List<string>();

        public BPlayer(string name)
        {
            this.name = name;
        }
    }
}
