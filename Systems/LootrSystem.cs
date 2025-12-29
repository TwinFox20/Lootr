using System;
using System.Collections.Generic;
using LootrMod.Config;
using LootrMod.DataStructures;
using LootrMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LootrMod.Systems;

public class LootrSystem : ModSystem
{
	public static readonly Dictionary<Point16, LootrChest> LootrChests = [];

	public override void PostWorldGen()
	{
		for (ushort i = 0; i < Main.maxChests; i++)
		{
			var chest = Main.chest[i];
			if (chest != null) TryRegisterLootrChest(chest);
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		var serializedChests = new List<TagCompound>();
		foreach (var (position, lootrChest) in LootrChests)
		{
			var chestTag = lootrChest.Save();
			chestTag["position"] = position;
			serializedChests.Add(chestTag);
		}
		tag["lootr_chests"] = serializedChests;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		LootrChests.Clear();
		if (!tag.ContainsKey("lootr_chests")) return;

		foreach (var chestTag in tag.GetList<TagCompound>("lootr_chests"))
		{
			var position = chestTag.Get<Point16>("position");
			LootrChests[position] = LootrChest.Load(chestTag);
		}
	}

	public static bool TryGetLootrChest(int chestIndex, out Chest chest, out LootrChest lootrChest)
	{
		lootrChest = null;
		chest = Main.chest[chestIndex];
		return chest != null && LootrChests.TryGetValue(new Point16(chest.x, chest.y), out lootrChest);
	}

	public static void HandleChestOpened(int chestIndex, int player)
	{
		if (!Main.dedServ && Main.netMode != NetmodeID.SinglePlayer) return;
		if (LootrConfig.Instance.DebugMode) Console.WriteLine($"Chest '{chestIndex}' opened by player '{player}'\n");

		if (!TryGetLootrChest(chestIndex, out var chest, out var lootrChest)) return;
		var playerId = Guid.Empty;
		lootrChest.HandleChestOpened(chest, playerId);
	}

	public static void HandleChestClosed(int chestIndex, int player)
	{
		if (!Main.dedServ && Main.netMode != NetmodeID.SinglePlayer) return;
		if (!TryGetLootrChest(chestIndex, out var chest, out var lootrChest)) return;
		var playerId = Guid.Empty;
		lootrChest.HandleChestClosed(chest, playerId);
	}

	private static void TryRegisterLootrChest(Chest chest)
	{
		if (chest.item.IsAir()) return;
		var position = new Point16(chest.x, chest.y);
		LootrChests[position] = new LootrChest(chest.item);
		chest.name = "Lootr Chest";
	}
}
