using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LootrMod.Utilities
{
	internal class NetworkUtilities
	{
		public static void WriteItems(ModPacket p, Item[] items)
		{
			var length = (byte)items.Length;
			p.Write(length);
			for (byte i = 0; i < length; i++)
				ItemIO.Send(items[i], p, true, false);
		}

		public static Item[] ReadItems(BinaryReader r)
		{
			var length = r.ReadByte();
			var items = new Item[length];
			for (byte i = 0; i < length; i++)
				items[i] = ItemIO.Receive(r, true, false);
			return items;
		}
	}
}
