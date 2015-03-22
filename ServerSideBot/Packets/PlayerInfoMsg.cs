using System.IO;
using Terraria;
using TShockAPI.Net;

namespace ServerSideBot.Packets
{
	public class PlayerInfoMsg : BaseMsg
	{
		public byte Index { get; set; }
		public byte Hair { get; set; }
		public byte Male { get; set; }
		public string Text { get; set; }
		public byte HairDye { get; set; }
		public byte HairColorR { get; set; }
		public byte HairColorG { get; set; }
		public byte HairColorB { get; set; }
		public byte SkinColorR { get; set; }
		public byte SkinColorG { get; set; }
		public byte SkinColorB { get; set; }
		public byte EyeColorR { get; set; }
		public byte EyeColorG { get; set; }
		public byte EyeColorB { get; set; }
		public byte ShirtColorR { get; set; }
		public byte ShirtColorG { get; set; }
		public byte ShirtColorB { get; set; }
		public byte UnderShirtColorR { get; set; }
		public byte UnderShirtColorG { get; set; }
		public byte UnderShirtColorB { get; set; }
		public byte PantsColorR { get; set; }
		public byte PantsColorG { get; set; }
		public byte PantsColorB { get; set; }
		public byte ShoeColorR { get; set; }
		public byte ShoeColorG { get; set; }
		public byte ShoeColorB { get; set; }
		public byte Difficulty { get; set; }

		public override PacketTypes ID
		{
			get { return PacketTypes.PlayerInfo; }
		}

		public override void Pack(Stream stream)
		{
			var writer = new BinaryWriter(stream);
			writer.Write(Index);
			writer.Write(Male);
			writer.Write(Hair);
			writer.Write(Text);
			writer.Write(HairDye);
			//Don't hide any visuals
			var hideVisual = new BitsByte(false);
			writer.Write(hideVisual);
			writer.WriteRGB(new Color(HairColorR, HairColorG, HairColorB));
			writer.WriteRGB(new Color(SkinColorR, SkinColorG, SkinColorB));
			writer.WriteRGB(new Color(EyeColorR, EyeColorG, EyeColorB));
			writer.WriteRGB(new Color(ShirtColorR, ShirtColorG, ShirtColorB));
			writer.WriteRGB(new Color(UnderShirtColorR, UnderShirtColorG, UnderShirtColorB));
			writer.WriteRGB(new Color(PantsColorR, PantsColorG, PantsColorB));
			writer.WriteRGB(new Color(ShoeColorR, ShoeColorG, ShoeColorB));
			writer.Write(Difficulty);
		}
	}
}
