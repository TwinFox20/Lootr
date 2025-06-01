using System.IO;
using LootrMod.Config;
using LootrMod.Networking;
using Terraria.ModLoader;

namespace LootrMod;

internal class LootrMod : Mod
{
	public static LootrMod Instance;

	public static ModKeybind DisplayFieldsKeybind;
	public static ModKeybind CreateLootrChestKeybind;
	public static ModKeybind RestoreLootrChestKeybind;

	public override void Load()
	{
		Instance = this;

		if (!LootrConfig.Instance.DebugMode) return;

		DisplayFieldsKeybind = KeybindLoader.RegisterKeybind(this, "DisplayFields", "I");
		CreateLootrChestKeybind = KeybindLoader.RegisterKeybind(this, "CreateLootrChest", "L");
		RestoreLootrChestKeybind = KeybindLoader.RegisterKeybind(this, "RestoreLootrChestKeybind", "O");
	}

	public override void Unload() => Instance = null;

	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		LootrNetwork.HandlePacket(reader, whoAmI);
	}
}
