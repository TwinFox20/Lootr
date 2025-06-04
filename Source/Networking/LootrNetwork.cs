using System.IO;
using LootrMod.Networking.Chests;
using LootrMod.Networking.Players;

namespace LootrMod.Networking;

public static class LootrNetwork
{
	public static void HandlePacket(BinaryReader reader, int sender)
	{
		var id = reader.ReadByte();
		switch (id)
		{
			case byte i when i < 100: PlayersNetwork.HandlePacket(id, reader, sender); break;
			case byte i when i >= 100: ChestsNetwork.HandlePacket(id, reader, sender); break;
			default: throw new System.NotImplementedException();
		};
	}
}
