using System;
using System.IO;
using LootrMod.DataStructures;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria;
using Terraria.ID;

namespace LootrMod.Networking.Chests;

public static class ChestsNetwork
{
	public enum PacketType : byte
	{
		ChestOpen    = 100,
		ChestClose   = 101,
		ChestRestore = 102
	}

	public static void HandlePacket(byte id, BinaryReader reader, int sender)
	{
		switch ((PacketType)id)
		{
			case PacketType.ChestOpen:    HandleChestOpen(reader, sender);    break; // Client -> Server -> Clients
			case PacketType.ChestClose:   HandleChestClose(reader, sender);   break; // Client -> Server -> Clients
			case PacketType.ChestRestore: HandleChestRestore(reader, sender); break; // Client -> Server
		}
	}

	public static void SendChestOpen(short chestIndex, int player)
	{
		var p = NetworkUtilities.CreatePacket(PacketType.ChestOpen, 2, player);
		p.Write(chestIndex);
		p.Send(-1, player);
	}

	public static void SendChestClose(short chestIndex, int player)
	{
		var p = NetworkUtilities.CreatePacket(PacketType.ChestClose, 2, player);
		p.Write(chestIndex);
		p.Send(-1, player);
	}

	public static void SendChestRestore(short chestIndex, int player)
	{
		var p = NetworkUtilities.CreatePacket(PacketType.ChestRestore);
		p.Write(chestIndex);
		p.Send(-1, player);
	}

	private static void HandleChestOpen(BinaryReader reader, int sender)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
			sender = reader.ReadByte();

		var chestIndex = reader.ReadInt16();
		if (Main.dedServ)
			SendChestOpen(chestIndex, sender);
		else if (LootrSystem.TryGetLootrChest(chestIndex, out var chest, out var lootrChest))
			lootrChest.FillChestWithPlayerItems(sender, chest);
	}

	private static void HandleChestClose(BinaryReader reader, int sender)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
			sender = reader.ReadByte();

		var chestIndex = reader.ReadInt16();
		if (Main.dedServ)
			SendChestClose(chestIndex, sender);
		else if (LootrSystem.TryGetLootrChest(chestIndex, out var chest, out var lootrChest))
			lootrChest.SavePlayerItems(sender, chest.item);
	}

	private static void HandleChestRestore(BinaryReader reader, int sender)
	{
		if (Main.dedServ)
		{
			var chestIndex = reader.ReadInt16();
			LootrSystem.TryGetLootrChest(chestIndex, out var _, out var lootrChest);
			lootrChest.playerRestoreTime.Remove(sender);
			SendChestRestore(chestIndex, sender);
		}
	}
}
