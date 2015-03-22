using System.IO;
using TShockAPI.Net;

namespace ServerSideBot.Packets
{
	public class PlayerActiveMsg : BaseMsg
	{
		public byte Index { get; set; }
		public const byte Active = 1;

		public override PacketTypes ID
		{
			get { return PacketTypes.PlayerActive; }
		}

		public override void Pack(Stream stream)
		{
			var writer = new BinaryWriter(stream);
			writer.Write(Index);
			writer.Write(Active);
		}
	}
}
