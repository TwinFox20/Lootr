using System;
using System.IO;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria;
using Terraria.ID;

namespace LootrMod.Networking;

public static class LootrNetwork
{
	private enum PacketType : byte
	{
		HandleGuid = 0,
		//SubstrTimers = 1
	}

	public static void HandlePacket(BinaryReader reader, int sender)
	{
		switch ((PacketType)reader.ReadByte())
		{
			case PacketType.HandleGuid: HandleGuid(reader, sender); break; // Client -> Server
			//case PacketType.SubstrTimers: HandleSubstrTimers(reader, sender); break; // Client -> Server
			default: throw new NotImplementedException();
		}
	}

	#region Guid

	public static void SendGuidToHandle(this Player _, Guid guid)
	{
		var packet = LootrMod.Instance.GetPacket(16);
		// var packet = NetworkUtilities.CreatePacket(PacketType.HandleGuid, 16);
		packet.Write((byte)0);
		packet.Write(guid.ToString("N"));
		packet.Send();
	}

	private static void HandleGuid(BinaryReader reader, int sender)
	{
		if (!Main.dedServ) return;
		var guid = Guid.Parse(reader.ReadString());
		if (guid.IsEmpty() || UniqueSystem.HasGuid(guid)) Netplay.Clients[sender].Reset();
		else UniqueSystem.SetGuid(sender, guid);
	}

	#endregion

	#region RestoreTime

	public static void SendSubtractTimers(this Player _, uint deltaTime)
	{
		// if (Main.netMode != NetmodeID.MultiplayerClient) return;
		// var p = NetworkUtilities.CreatePacket(PacketType.SubstrTimers, 4);
		// p.Write(deltaTime);
		// p.Send();
	}

	private static void HandleSubstrTimers(BinaryReader reader, int sender)
	{
		if (!Main.dedServ) return;
		var deltaTime = reader.ReadUInt32();
		var player = UniqueSystem.GetGuid(sender);
		foreach (var (_, chest) in LootrSystem.LootrChests)
		{
			var restoreTimers = chest.RestoreTimers;
			if (!restoreTimers.TryGetValue(player, out var timeToRestore)) return;

			if (timeToRestore <= deltaTime) restoreTimers.Remove(player);
			else restoreTimers[player] -= deltaTime;
		}
	}

	#endregion
}