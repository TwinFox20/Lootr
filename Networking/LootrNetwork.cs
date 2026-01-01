using System;
using System.IO;
using LootrMod.Config;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria;
using Terraria.ID;

namespace LootrMod.Networking;

public static class LootrNetwork
{
	private enum PacketType : byte
	{
		Guid = 0,
		SubstrTimers = 1
	}

	public static void HandlePacket(BinaryReader reader, int sender)
	{
		var packetType = (PacketType)reader.ReadByte();
		switch (packetType)
		{
			case PacketType.Guid: HandleGuid(reader, sender); break; // Client -> Server
			case PacketType.SubstrTimers: HandleSubstrTimers(reader, sender); break; // Client -> Server
			default: throw new Exception($"Unhandled packet type {packetType}");
		}
	}

	#region Guid
	public static void SendGuidToServer(this Player player, Guid guid)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient) return;
		var packet = LootrMod.Instance.GetPacket(17);
		packet.Write((byte)0);
		packet.Write(guid.ToString("N"));
		packet.Send(-1, player.whoAmI);
		if (LootrConfig.Instance.Debug) Console.WriteLine($"Sending guid as {player.whoAmI}: {guid}");
	}

	private static void HandleGuid(BinaryReader reader, int sender)
	{
		if (!Main.dedServ) return;
		var guid = Guid.Parse(reader.ReadString());
		var valid = !guid.IsEmpty() && !UniquePlayerLib.HasGuid(guid);
		if (valid) UniquePlayerLib.SetGuid(sender, guid);
		else Netplay.Clients[sender].Reset();
		if (LootrConfig.Instance.Debug) Console.WriteLine($"{(valid ? "Received" : "Rejected")} guid from {sender}: {guid}");
	}
	#endregion

	#region Subtract timers
	public static void SendSubtractTimers(this Player player, uint deltaTime)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient) return;
		var packet = LootrMod.Instance.GetPacket(5);
		packet.Write((byte)1);
		packet.Write(deltaTime);
		packet.Send(-1, player.whoAmI);
	}

	private static void HandleSubstrTimers(BinaryReader reader, int sender)
	{
		if (!Main.dedServ) return;
		var deltaTime = reader.ReadUInt32();
		var guid = UniquePlayerLib.GetGuid(sender);
		// TODO: make per-player list of lootr chest's positions
		foreach (var (_, chest) in LootrSystem.LootrChests)
		{
			var restoreTimers = chest.RestoreTimers;
			if (!restoreTimers.TryGetValue(guid, out var timeToRestore)) continue;

			if (timeToRestore <= deltaTime) restoreTimers.Remove(guid);
			else restoreTimers[guid] -= deltaTime;
		}
	}
	#endregion
}
