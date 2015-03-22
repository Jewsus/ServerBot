using System.Runtime.CompilerServices;
using TShockAPI;

namespace ServerSideBot.Extensions
{
	internal static class PlayerExtensions
	{
		private static readonly ConditionalWeakTable<TSPlayer, BotUser> Players = 
			new ConditionalWeakTable<TSPlayer, BotUser>();

		internal static BotUser GetBotUser(this TSPlayer tsplayer)
		{
			return Players.GetValue(tsplayer, ah => new BotUser());
		}
	}
}
