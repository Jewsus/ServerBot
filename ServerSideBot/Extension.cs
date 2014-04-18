using System.Collections.Generic;
using System.Globalization;
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

        public static void AddFlag(this Channel chan, char c, bool final = false)
        {
            if (c != 's' || c != 'm' || c != 'i' || c != 'r' || c != 'k' || c != 'l')
                return;

            if (chan.modes.Contains(c.ToString(CultureInfo.InvariantCulture)))
                return;

            chan.modes += c;

            if (final)
                SSBot.Database.UpdateChannel(chan);
        }

        public static void DelFlag(this Channel chan, char c, bool final = false)
        {
            if (c != 's' && c != 'm' && c != 'i' && c != 'r' && c != 'k' && c != 'l')
                return;

            if (!chan.modes.Contains(c.ToString(CultureInfo.InvariantCulture)))
                return;

            chan.modes = chan.modes.Remove(chan.modes.IndexOf(c), 1);

            if (final)
                SSBot.Database.UpdateChannel(chan);
        }

        public static void AddEx(this List<Channel> list, Channel chan)
        {
            if (list.Contains(chan))
                return;

            list.Add(chan);
            SSBot.Database.InsertChannel(chan);
        }
    }
}
