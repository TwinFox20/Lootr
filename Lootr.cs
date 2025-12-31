using System.IO;
using LootrMod.Networking;
using Terraria.ModLoader;

namespace LootrMod;

public class LootrMod : Mod
{
	public static LootrMod Instance;

	public override void Load() => Instance = this;

	public override void Unload() => Instance = null;

	public override void HandlePacket(BinaryReader reader, int whoAmI) => LootrNetwork.HandlePacket(reader, whoAmI);
}
