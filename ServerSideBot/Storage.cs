using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace ServerSideBot
{
    public class Storage
    {
        public List<BPlayer> players = new List<BPlayer>();

        public BPlayer GetPlayerByName(string name)
        {
            return players.FirstOrDefault(player => player.name == name) != null
                ? players.FirstOrDefault(player => player.name == name)
                : null;
        }

        public List<BPlayer> GetPlayerListByName(string name)
        {
            name = name.ToLower();
            var retList = new List<BPlayer>();

            foreach (BPlayer player in players)
            {
                if (player.name.ToLower() == name)
                    return new List<BPlayer> {player};
                if (player.name.ToLower().Contains(name))
                    retList.Add(player);
            }

            return retList;
        }

        public List<User> GetTSUsersListByName(string name)
        {
            name = name.ToLower();
            var retList = new List<User>();

            foreach (User user in TShock.Users.GetUsers())
            {
                if (user.Name.ToLower() == name)
                    return new List<User> {user};
                if (user.Name.ToLower().Contains(name))
                    retList.Add(user);
            }

            return retList;
        }
    }
}
