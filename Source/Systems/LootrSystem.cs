using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
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

	public static bool TryGetLootrChest(int chestIndex, out Chest chest, out LootrChest lootrChest)
	{
		lootrChest = default;
		chest = Main.chest[chestIndex];
		if (chest == null) return false;
		return lootrChests.TryGetValue(new Point16(chest.x, chest.y), out lootrChest);
	}

	public static void OnChestOpen(int chestIndex, int player)
	{
		if (Main.dedServ || Main.netMode == NetmodeID.SinglePlayer)
		{
			if (chestIndex < 0) return;
			if (LootrConfig.Instance.DebugMode)
				Console.WriteLine($"Chest '{chestIndex}' opened by player '{player}'\n");

			if (!TryGetLootrChest(chestIndex, out var chest, out var lootrChest)) return;
			lootrChest.OnChestOpen(chest, player);
			ShowLootrChestDebugInfo(lootrChest, player);
		}
	}

	public static void OnChestClose(int chestIndex, int player)
	{
		if (Main.dedServ || Main.netMode == NetmodeID.SinglePlayer)
		{
			if (!TryGetLootrChest(chestIndex, out var chest, out var lootrChest)) return;
			lootrChest.OnChestClose(chest, player);
		}
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

	private static void ShowLootrChestDebugInfo(LootrChest lootrChest, int player)
	{
		if (!LootrConfig.Instance.DebugMode) return;

		var logger = $"World gen. items: {lootrChest.worldGenItems.Select(i => i.Name).Humanize()}\n";

		if (lootrChest.playerItems.TryGetValue(player, out var playeritems) && !playeritems.All(item => item.IsAir))
			logger += $"Player items: {playeritems.Select(i => i.Name).Humanize()}\n";

		if (lootrChest.playerRestoreTime.TryGetValue(player, out var timeToRestore))
		{
			var timeSpan = TimeSpan.FromSeconds((timeToRestore - Main.GameUpdateCount) / 60);
			logger += $"Remaining time to restore: {timeSpan:hh\\:mm\\:ss}\n";
		}

		Console.WriteLine(logger);
	}
}
