using System.Collections.Generic;
using System.Linq;
using LootrMod.DataStructures;
using LootrMod.Networking.Chests;
using LootrMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LootrMod.Systems;

public class LootrSystem : ModSystem
{
	public static Dictionary<Point16, LootrChest> lootrChests = [];

	public override void PostWorldGen()
	{
		for (ushort i = 0; i < Main.maxChests; i++)
		{
			var chest = Main.chest[i];
			if (chest != null)
				TryRegisterLootrChest(chest);
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		var serializedChests = new List<TagCompound>();
		foreach (var (position, lootrChest) in lootrChests)
		{
			var chestTag = lootrChest.Save();
			chestTag["position"] = position;
			serializedChests.Add(chestTag);
		}
		tag["LootrChests"] = serializedChests;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		lootrChests.Clear();
		if (!tag.ContainsKey("LootrChests")) return;

		foreach (var chestTag in tag.GetList<TagCompound>("LootrChests"))
		{
			var position = chestTag.Get<Point16>("position");
			lootrChests[position] = LootrChest.Load(chestTag);
		}
	}

	public static bool TryGetLootrChest(short chestIndex, out Chest chest, out LootrChest lootrChest)
	{
		lootrChest = default;
		chest = Main.chest[chestIndex];
		if (chest == null) return false;
		return lootrChests.TryGetValue(new Point16(chest.x, chest.y), out lootrChest);
	}

	public static void OnChestOpened(short chestIndex, int player)
	{
		if (!TryGetLootrChest(chestIndex, out var _, out var _)) return;

		if (Main.netMode == NetmodeID.MultiplayerClient)
			ChestsNetwork.SendChestOpen(chestIndex, player);
		//lootrChest.FillChestWithPlayerItems(player, chest);
	}

	public static void OnChestClosed(short chestIndex, int player)
	{
		if (!TryGetLootrChest(chestIndex, out var _, out var _)) return;

		if (Main.netMode == NetmodeID.MultiplayerClient)
			ChestsNetwork.SendChestClose(chestIndex, player);
		//lootrChest.SavePlayerItems(player, chest.item);
	}

	private static void TryRegisterLootrChest(Chest chest)
	{
		if (chest.item.All(item => item.IsAir))
			return;
		
		var position = new Point16(chest.x, chest.y);
		lootrChests[position] = new LootrChest
		{
			worldGenItems = LootrUtilities.DeepCloneItems(chest.item)
		};
		chest.name = "Lootr Chest";
	}
}
