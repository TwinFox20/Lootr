using System.Collections.Generic;
using System.Linq;
using LootrMod.Config;
using LootrMod.Utilities;
using Terraria;
using Terraria.ModLoader.IO;

namespace LootrMod.DataStructures;

public class LootrChest
{
	public Item[] worldGenItems = [];
	public Dictionary<int, Item[]> playerItems = [];
	public Dictionary<int, uint> playerRestoreTime = [];

	public TagCompound Save()
	{
		var tag = new TagCompound
		{
			["worldGenItems"] = LootrUtilities.WriteItems(worldGenItems),
			["playerItems"] = playerItems.Select(pair => new TagCompound
			{
				["player"] = pair.Key,
				["items"] = LootrUtilities.WriteItems(pair.Value)
			}).ToList(),

			["playerRestoreTime"] = playerRestoreTime.Select(pair => new TagCompound
			{
				["player"] = pair.Key,
				["time"] = pair.Value
			}).ToList()
		};

		return tag;
	}

	public static LootrChest Load(TagCompound tag)
	{
		var chest = new LootrChest
		{
			worldGenItems = LootrUtilities.ReadItems(tag.GetList<TagCompound>("worldGenItems"))
		};

		foreach (var entry in tag.GetList<TagCompound>("playerItems"))
		{
			int player = entry.GetInt("player");
			chest.playerItems[player] = LootrUtilities.ReadItems(entry.GetList<TagCompound>("items"));
		}

		foreach (var entry in tag.GetList<TagCompound>("playerRestoreTime"))
		{
			int player = entry.GetInt("player");
			chest.playerRestoreTime[player] = entry.Get<uint>("time");
		}

		return chest;
	}

	public void FillChestWithPlayerItems(int player, Chest chest)
	{
		OnFirstOpen(player);
		TryToRestore(player);
		chest.item = LootrUtilities.DeepCloneItems(playerItems[player], false);
	}

	public void SavePlayerItems(int player, Item[] items)
	{
		playerItems[player] = LootrUtilities.DeepCloneItems(items);
		TrySheduleRestore(player);
	}

	private void OnFirstOpen(int player)
	{
		if (!playerItems.ContainsKey(player) && !playerRestoreTime.ContainsKey(player))
			playerItems[player] = LootrUtilities.DeepCloneItems(worldGenItems);
	}

	private void TryToRestore(int player)
	{
		if (!playerRestoreTime.TryGetValue(player, out uint restoreTime)) return;
		var playerTimeInWorld = Main.player[player].GetModPlayer<LootrPlayer.LootrPlayer>().TimeInWorld;
		if (restoreTime > playerTimeInWorld) return;

		playerRestoreTime.Remove(player);
		playerItems[player] = LootrUtilities.DeepCloneItems(worldGenItems);
	}

	private void TrySheduleRestore(int player)
	{
		if (!LootrConfig.Instance.AllowRestore || playerRestoreTime.ContainsKey(player) || playerItems.ContainsKey(player)) return;
		playerRestoreTime[player] = (uint)LootrConfig.Instance.SecondsToRestore * 60;
	}
}
