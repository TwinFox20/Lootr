using System.IO;
using LootrMod.DataStructures;
using LootrMod.Systems;
using LootrMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LootrMod.Networking
{
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
				case PacketType.DataSync:   HandleDataSync(reader, sender);   break;
				case PacketType.ChestOpen:  HandleChestOpen(reader, sender);  break;
				case PacketType.ChestClose: HandleChestClose(reader, sender); break;
			}
		}

		public static void SendDataToPlayer(int player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				var p = CreatePacket(PacketType.DataSync, player);
				p.Send(-1, player);
			}

			if (Main.netMode == NetmodeID.Server)
			{
				var p = CreatePacket(PacketType.DataSync, player);
				p.Write((ushort)LootrSystem.lootrChests.Count);

				foreach (var (position, lootrChest) in LootrSystem.lootrChests)
				{
					p.Write((short)position.X);
					p.Write((short)position.Y);
					NetworkUtilities.WriteItems(p, lootrChest.worldGenItems);
				}

				p.Send(player);
			}
		}

		public static void SendChestOpen(int chestIndex, int player)
		{
			var p = CreatePacket(PacketType.ChestOpen, player);
			p.Write((short)chestIndex);
			p.Send(-1, player);
		}

		public static void SendChestClose(int chestIndex, int player)
		{
			var p = CreatePacket(PacketType.ChestClose, player);
			p.Write((short)chestIndex);
			p.Send(-1, player);
		}

		private static void HandleDataSync(BinaryReader reader, int player)
		{
			if (Main.netMode == NetmodeID.Server)
				SendDataToPlayer(player);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				LootrSystem.lootrChests.Clear();

				ushort chestCount = reader.ReadUInt16();
				for (int i = 0; i < chestCount; i++)
				{
					int x = reader.ReadInt16();
					int y = reader.ReadInt16();
					Item[] items = NetworkUtilities.ReadItems(reader);
					LootrSystem.lootrChests[new Point(x, y)] = new LootrChest { worldGenItems = items };
				}
			}
		}

		private static void HandleChestOpen(BinaryReader reader, int sender)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				sender = reader.ReadByte();

			int chestIndex = reader.ReadInt16();

			if (Main.netMode == NetmodeID.Server)
				SendChestOpen(chestIndex, sender);

			else if (LootrSystem.TryGetLootrChest(chestIndex, out var chest, out var lootrChest))
				lootrChest.FillChestWithPlayerItems(sender, chest);
		}

		private static void HandleChestClose(BinaryReader reader, int sender)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				sender = reader.ReadByte();

			int chestIndex = reader.ReadInt16();

			if (Main.netMode == NetmodeID.Server)
				SendChestClose(chestIndex, sender);

			else if (LootrSystem.TryGetLootrChest(chestIndex, out var chest, out var lootrChest))
				lootrChest.SavePlayerItems(sender, chest.item);
		}

		private static ModPacket CreatePacket(PacketType type, int sender)
		{
			var packet = LootrMod.Instance.GetPacket();
			packet.Write((byte)type);

			if (Main.netMode == NetmodeID.Server)
				packet.Write((byte)sender);

			return packet;
		}
	}
}
