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
		for (var i = 0; i < Main.maxChests; i++)
		{
			var chest = Main.chest[i];
			if (chest == null) continue;
			TryRegisterLootrChest(chest);
		}
	}

	#region NBT
	public override void SaveWorldData(TagCompound tag)
	{
		var serializedChests = new List<TagCompound>();
		foreach (var (position, lootrChest) in LootrChests)
		{
			var chestTag = lootrChest.Save();
			chestTag["position"] = position;
			serializedChests.Add(chestTag);
		}
		tag["lootrChests"] = serializedChests;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		LootrChests.Clear();
		if (!tag.ContainsKey("lootrChests")) return;

		foreach (var chestTag in tag.GetList<TagCompound>("lootrChests"))
		{
			var position = chestTag.Get<Point16>("position");
			LootrChests[position] = LootrChest.Load(chestTag);
		}
	}
	#endregion

	public static void GetLootrChest(int chestIndex, out Chest chest, out LootrChest lootrChest)
	{
		chest = Main.chest[chestIndex];
		if (chest == null) throw new ArgumentNullException(nameof(chest));
		LootrChests.TryGetValue(new Point16(chest.x, chest.y), out lootrChest);
	}

	public static void HandleChestOpened(int chestIndex, int player)
	{
		if (!Main.dedServ && Main.netMode != NetmodeID.SinglePlayer) return;
		if (LootrConfig.Instance.Debug)
			Console.WriteLine($"Chest '{chestIndex}' was opened by player '{player}'\n");
		GetLootrChest(chestIndex, out var chest, out var lootrChest);
		var guid = UniquePlayerLib.GetGuid(player);
		lootrChest.HandleChestOpened(chest, guid);
	}

	public static void HandleChestClosed(int chestIndex, int player)
	{
		if (!Main.dedServ && Main.netMode != NetmodeID.SinglePlayer) return;
		GetLootrChest(chestIndex, out var chest, out var lootrChest);
		var guid = UniquePlayerLib.GetGuid(player);
		lootrChest.HandleChestClosed(chest, guid);
	}

	private static void TryRegisterLootrChest(Chest chest)
	{
		if (chest.item.IsAir()) return;
		var position = new Point16(chest.x, chest.y);
		LootrChests[position] = new LootrChest(chest.item);
		chest.name = "Lootr Chest";
	}
}
