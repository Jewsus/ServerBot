using System.IO;
using Terraria;
using TShockAPI.Net;

namespace ServerSideBot.Packets
{
	public class PlayerUpdateMsg : BaseMsg
	{
		public byte Index { get; set; }
		public bool ControlUp { get; set; }
		public bool ControlDown { get; set; }
		public bool ControlLeft { get; set; }
		public bool ControlRight { get; set; }
		public bool ControlJump { get; set; }
		public bool ControlUseItem { get; set; }
		public bool PlayerDirection { get; set; }
		public bool UsingPulley { get; set; }
		public byte PulleyDirection { get; set; }
		public byte HeldItem { get; set; }
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }

		public override PacketTypes ID
		{
			get { return PacketTypes.PlayerUpdate; }
		}

		public override void Pack(Stream stream)
		{
			var writer = new BinaryWriter(stream);
			writer.Write(Index);
			var bitsByte1 = new BitsByte(ControlUp, ControlDown, ControlLeft, ControlRight, ControlJump,
				ControlUseItem, PlayerDirection);
			writer.Write(bitsByte1);
			var bitsByte2 = new BitsByte(UsingPulley, PulleyDirection == 2, Velocity != Vector2.Zero);
			writer.Write(bitsByte2);
			writer.Write(HeldItem);
			writer.WriteVector2(Position);
			writer.WriteVector2(Velocity);
		}
	}
}
