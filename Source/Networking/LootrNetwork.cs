using System;
using System.IO;
using LootrMod.DataStructures;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LootrMod.Networking;

public static class LootrNetwork
{
	private enum PacketType : byte
	{
		DataSync,
		ChestOpen,
		ChestClose
	}

	public static void HandlePacket(BinaryReader reader, int sender)
	{
		switch ((PacketType)reader.ReadByte())
		{
			case PacketType.DataSync:   HandleDataSync(reader, sender);   break; // Invoke -> Handle -> Return
			case PacketType.ChestOpen:  HandleChestOpen(reader, sender);  break; // Invoke -> Handle -> Spread
			case PacketType.ChestClose: HandleChestClose(reader, sender); break; // Invoke -> Handle -> Spread
		}
	}

	public static void SendDataToPlayer(int player)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			var p = CreatePacket(PacketType.DataSync);
			p.Send(-1, player);
		}

		if (Main.dedServ)
		{
			var p = CreatePacket(PacketType.DataSync);
			Console.WriteLine("LootrChestsCount: " + LootrSystem.lootrChests.Count);
			p.Write((ushort)LootrSystem.lootrChests.Count);

			foreach (var (position, lootrChest) in LootrSystem.lootrChests)
			{
				p.Write((ushort)position.X);
				p.Write((ushort)position.Y);
				NetworkUtilities.WriteItems(p, lootrChest.worldGenItems);
			}

			p.Send(player);
		}
	}

	public static void SendChestOpen(int chestIndex, int player)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			var p = CreatePacket(PacketType.ChestOpen, player);
			p.Write((short)chestIndex);
			p.Send(-1, player);
		}
	}

	public static void SendChestClose(int chestIndex, int player)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			var p = CreatePacket(PacketType.ChestClose, player);
			p.Write((short)chestIndex);
			p.Send(-1, player);
		}
	}

	private static void HandleDataSync(BinaryReader reader, int sender)
	{
		if (Main.dedServ)
			SendDataToPlayer(sender);

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			LootrSystem.lootrChests.Clear();

			var chestCount = reader.ReadUInt16();
			for (ushort i = 0; i < chestCount; i++)
			{
				var x = reader.ReadUInt16();
				var y = reader.ReadUInt16();
				var items = NetworkUtilities.ReadItems(reader);
				LootrSystem.lootrChests[new Point16(x, y)] = new LootrChest { worldGenItems = items };
			}
		}
	}

	private static void HandleChestOpen(BinaryReader reader, int sender)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
			sender = reader.ReadByte();

		int chestIndex = reader.ReadInt16();

		if (Main.dedServ)
			SendChestOpen(chestIndex, sender);

		else if (LootrSystem.TryGetLootrChest(chestIndex, out var chest, out var lootrChest))
			lootrChest.FillChestWithPlayerItems(sender, chest);
	}

	private static void HandleChestClose(BinaryReader reader, int sender)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
			sender = reader.ReadByte();

		int chestIndex = reader.ReadInt16();

		if (Main.dedServ)
			SendChestClose(chestIndex, sender);

		else if (LootrSystem.TryGetLootrChest(chestIndex, out var chest, out var lootrChest))
			lootrChest.SavePlayerItems(sender, chest.item);
	}

	/// <summary>
	/// Create a <see cref="ModPacket"/> to write the data in and sending
	/// </summary>
	/// <param name="type"></param>
	/// <param name="sender">Player ID in <see cref="Main.player"/></param>
	/// <returns><see cref="ModPacket"/> with type(<see cref="byte"/>) and optional with sender(<see cref="byte"/>)</returns>
	private static ModPacket CreatePacket(PacketType type, int sender = -1)
	{
		var packet = LootrMod.Instance.GetPacket();
		packet.Write((byte)type);

		if (Main.dedServ && sender != -1)
			packet.Write((byte)sender);

		return packet;
	}
}
