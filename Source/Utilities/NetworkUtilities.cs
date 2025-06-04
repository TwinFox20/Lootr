using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LootrMod.Utilities;

internal class NetworkUtilities
{
	/// <summary>
	/// Create a <see cref="ModPacket"/> to write the data in and sending
	/// </summary>
	/// <param name="packetType">Type of a packet</param>
	/// <param name="sender">Player ID in <see cref="Main.player"/></param>
	/// <returns><see cref="ModPacket"/> with type(<see cref="byte"/>) and optional with sender(<see cref="byte"/>)</returns>
	public static ModPacket CreatePacket<T>(T packetType, int capacity = 256, int sender = -1) where T: Enum
	{
		var packet = LootrMod.Instance.GetPacket(capacity);
		var id = Enum.Parse(typeof(T), packetType.ToString());
		packet.Write((byte)id);

		if (Main.dedServ && sender != -1)
			packet.Write((byte)sender);

		return packet;
	}

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
