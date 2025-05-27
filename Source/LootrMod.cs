using System.IO;
using LootrMod.Networking;
using Terraria.ModLoader;

namespace LootrMod
{
	internal class LootrMod : Mod
	{
		public static LootrMod Instance;

		public static ModKeybind DisplayFieldsKeybind;
		public static ModKeybind CreateLootrChestKeybind;

		public override void Load()
		{
			Instance = this;

			DisplayFieldsKeybind = KeybindLoader.RegisterKeybind(this, "DisplayFields", "I");
			CreateLootrChestKeybind = KeybindLoader.RegisterKeybind(this, "CreateLootrChest", "L");
		}

		public override void Unload() => Instance = null;

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			LootrNetwork.HandlePacket(reader, whoAmI);
		}
	}
}
