using System.Collections.Generic;
using Newtonsoft.Json.Schema;

namespace ServerSideBot
{
    public static class Extension
    {
        public static void RemoveEx(this List<string> list, BPlayer player, string value)
        {
            value = value.ToLower();
            if (!list.Contains(value))
                return;

            list.Remove(value);
            SSBot.Database.SavePlayer(player);
        }

        public static void AddEx(this List<string> list, BPlayer player, string value)
        {
            value = value.ToLower();
            if (list.Contains(value))
                return;

            list.Add(value);
            SSBot.Database.SavePlayer(player);
        }
    }
}
