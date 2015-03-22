using System.IO;
using System.Linq;
using ServerSideBot.Packets;
using Terraria;
using TShockAPI;

namespace ServerSideBot
{
    public class Bot
    {
		private string Name { get; set; }
	    private Color _chatColor;
	    internal Vector2 velocity = Vector2.Zero;
	    internal Vector2 position = new Vector2(Main.spawnTileX*16, Main.spawnTileY*16 - 48);

	    public Bot(string name)
	    {
		    Name = name;
	    }

	    public void SetChatColor(int[] rgbs)
	    {
		    _chatColor = new Color(rgbs[0], rgbs[1], rgbs[2]);
	    }

	    public void SendPlayerInfo()
	    {
		    using (var ms = new MemoryStream())
		    {
			    var msg = new PlayerInfoMsg
			    {
					Index = 254,
					Hair = 21,
					Male = 0,
					Text = Name,
					HairDye = 0,
					HairColorR = _chatColor.R,
					HairColorG = _chatColor.G,
					HairColorB = _chatColor.B,
					SkinColorR = _chatColor.R,
					SkinColorG = _chatColor.G,
					SkinColorB = _chatColor.B,
					EyeColorR = _chatColor.R,
					EyeColorG = _chatColor.G,
					EyeColorB = _chatColor.B,
					ShirtColorR = _chatColor.R,
					ShirtColorG = _chatColor.G,
					ShirtColorB = _chatColor.B,
					UnderShirtColorR = _chatColor.R,
					UnderShirtColorG = _chatColor.G,
					UnderShirtColorB = _chatColor.B,
					PantsColorR = _chatColor.R,
					PantsColorG = _chatColor.G,
					PantsColorB = _chatColor.B,
					ShoeColorR = _chatColor.R,
					ShoeColorG = _chatColor.G,
					ShoeColorB = _chatColor.B,
					Difficulty = 0
			    };

				msg.PackFull(ms);
			    foreach (var player in TShock.Players.Where(p => p != null))
			    {
				    player.SendRawData(ms.ToArray());
			    }
		    }
	    }

	    public void SendPlayerActive()
	    {
		    using (var ms = new MemoryStream())
		    {
			    var msg = new PlayerActiveMsg
			    {
					Index = 254
			    };

			    msg.PackFull(ms);
				foreach (var player in TShock.Players.Where(p => p != null))
				{
					player.SendRawData(ms.ToArray());
				}
		    }
	    }

	    public void SendPlayerUpdate()
	    {
		    using (var ms = new MemoryStream())
		    {
			    var msg = new PlayerUpdateMsg
			    {
					Index = 254,
					ControlUp = false,
					ControlDown = false,
					ControlLeft = false,
					ControlRight = false,
					ControlJump = false,
					ControlUseItem = false,
					PlayerDirection = false,
					UsingPulley = false,
					PulleyDirection = 0,
					HeldItem = 0,
					Position = position,
					Velocity = velocity
			    };

				msg.PackFull(ms);
				foreach (var player in TShock.Players.Where(p => p != null))
				{
					player.SendRawData(ms.ToArray());
				}
		    }
	    }


		/// <summary>
		/// Global message to all players from the bot
		/// </summary>
		/// <param name="text">Text to be sent</param>
		public void Say(string text)
		{
			TSPlayer.All.SendMessage(string.Format("<{0}>: {1}", Name, text), _chatColor);
			TSPlayer.Server.SendMessage(string.Format("<{0}>: {1}", Name, text), _chatColor);
			TShock.Log.Info(string.Format("Botcast: {0}", text));
		}
		/// <summary>
		/// Global message to all players from the bot, with formatting
		/// </summary>
		/// <param name="text">Format to be sent</param>
		/// <param name="args">Format arguments</param>
		public void Say(string text, params object[] args)
		{
			TSPlayer.All.SendMessage(string.Format("<{0}>: {1}", Name,
				string.Format(text, args)), _chatColor);
			TSPlayer.Server.SendMessage(string.Format("<{0}>: {1}", Name,
				string.Format(text, args)), _chatColor);
			TShock.Log.Info(string.Format("Botcast: {0}", text));
		}

		/// <summary>
		/// Emulates a whisper sent from the bot
		/// </summary>
		/// <param name="text">Text to be sent</param>
		/// <param name="player">Player to send text to</param>
		public void PrivateSay(string text, TSPlayer player)
		{
			if (player == null || !player.ConnectionAlive || !player.Active)
				return;

			player.SendMessage(string.Format("<From {0}> {1}", Name, text), Color.MediumPurple);
		}
		/// <summary>
		/// Emulates a whisper sent from the bot
		/// </summary>
		/// <param name="text">Format to be sent</param>
		/// <param name="player">Player to send text to</param>
		/// <param name="args">Format arguments</param>
		public void PrivateSay(string text, TSPlayer player, params object[] args)
		{
			if (player == null || !player.ConnectionAlive || !player.Active)
				return;

			player.SendMessage(string.Format("<From {0}> {1}", Name,
				string.Format(text, args)), Color.MediumPurple);
		}
    }
}