namespace ServerSideBot.Commands
{
	public static class BuiltInCommands
	{
		public static void Say(BotCommandArgs args)
		{
			var words = string.Join(" ", args.Parameters);

			args.Bot.Say(words);

			args.handled = args.priv;
		}
	}
}