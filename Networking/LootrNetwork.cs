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
		SubstrTimers = 1
	}

	public static void HandlePacket(BinaryReader reader, int sender)
	{
		var packetType = (PacketType)reader.ReadByte();
		switch (packetType)
		{
			case PacketType.HandleGuid: HandleGuid(reader, sender); break; // Client -> Server
			case PacketType.SubstrTimers: HandleSubstrTimers(reader, sender); break; // Client -> Server
			default: throw new NotImplementedException();
		}
	}

	#region Guid
	public static void SendGuidToHandle(this Player player, Guid guid)
	{
		Console.WriteLine($"Sending guid to {player.name}: {guid}");
		var packet = LootrMod.Instance.GetPacket(16);
		packet.Write((byte)0);
		packet.Write(guid.ToString("N"));
		packet.Send();
	}

	private static void HandleGuid(BinaryReader reader, int sender)
	{
		var guid = Guid.Parse(reader.ReadString());
		Console.WriteLine($"Getting guid from {sender}: {guid}");
		if (!Main.dedServ) return;
		if (guid.IsEmpty() || UniquePlayerLib.HasGuid(guid)) Netplay.Clients[sender].Reset();
		else UniquePlayerLib.SetGuid(sender, guid);
		Console.WriteLine($"Sender {sender}: {guid}");
	}
	#endregion

	#region RestoreTime
	public static void SendSubtractTimers(this Player player, uint deltaTime)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient) return;
		var packet = LootrMod.Instance.GetPacket(4);
		packet.Write(deltaTime);
		packet.Send();
	}

	private static void HandleSubstrTimers(BinaryReader reader, int sender)
	{
		var deltaTime = reader.ReadUInt32();
		var player = UniquePlayerLib.GetGuid(sender);
		if (!Main.dedServ) return;
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