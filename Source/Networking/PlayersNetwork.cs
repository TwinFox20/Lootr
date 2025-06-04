using System.Collections.Generic;
using System.IO;
using LootrMod.DataStructures;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace LootrMod.Networking.Players;

public static class PlayersNetwork
{
	public enum PacketType : byte
	{
		DataSync     = 000,
		SubstrTimers = 001
	}

	public static void HandlePacket(byte id, BinaryReader reader, int sender)
	{
		switch ((PacketType)id)
		{
			case PacketType.DataSync:     HandleDataSync(reader, sender);     break; // Client -> Server -> Client
			case PacketType.SubstrTimers: HandleSubstrTimers(reader, sender); break; // Client -> Server
		}
	}

	public static void SendDataToPlayer(int player)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			var p = NetworkUtilities.CreatePacket(PacketType.DataSync, 0);
			p.Send(-1, player);
		}

		if (Main.dedServ)
		{
			var p = NetworkUtilities.CreatePacket(PacketType.DataSync);
			p.Write((ushort)LootrSystem.lootrChests.Count);
			foreach (var (position, lootrChest) in LootrSystem.lootrChests)
			{
				p.Write((ushort)position.X);
				p.Write((ushort)position.Y);
				NetworkUtilities.WriteItems(p, lootrChest.worldGenItems);
				lootrChest.playerRestoreTime.TryGetValue(player, out var timetoRestore);
				p.Write(timetoRestore == default ? 0 : timetoRestore);
			}

			p.Send(player);
		}
	}

	public static void SendSubstructTimers(int player, uint deltaTime)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			var p = NetworkUtilities.CreatePacket(PacketType.SubstrTimers, 4);
			p.Write(deltaTime);
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
				var worldGenItems = NetworkUtilities.ReadItems(reader);
				var restoreTimer = reader.ReadUInt32();

				var lootrChest = new LootrChest { worldGenItems = worldGenItems };
				if (restoreTimer != 0)
					lootrChest.playerRestoreTime = new Dictionary<int, uint> { { sender, restoreTimer } };

				LootrSystem.lootrChests[new Point16(x, y)] = lootrChest;
			}
		}
	}

	private static void HandleSubstrTimers(BinaryReader reader, int sender)
	{
		if (Main.dedServ)
		{
			var deltaTime = reader.ReadUInt32();
			foreach (var (_, chest) in LootrSystem.lootrChests)
			{
				var restoreTimers = chest.playerRestoreTime;
				if (!restoreTimers.TryGetValue(sender, out var timeToRestore)) return;

				if (timeToRestore <= deltaTime)
					restoreTimers.Remove(sender);
				else
					restoreTimers[sender] -= deltaTime;
			}
		}
	}
}
