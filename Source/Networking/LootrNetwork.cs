using System.IO;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria;
using Terraria.ID;

namespace LootrMod.Networking;

public static class LootrNetwork
{
	public enum PacketType : byte
	{
		SubstrTimers = 0
	}

	public static void HandlePacket(BinaryReader reader, int sender)
	{
		switch ((PacketType)reader.ReadByte())
		{
			case PacketType.SubstrTimers: HandleSubstrTimers(reader, sender); break; // Client -> Server
			default: throw new System.NotImplementedException();
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
