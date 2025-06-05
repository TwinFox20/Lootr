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

	public void OnChestOpen(Chest chest, int player)
	{
		OnFirstOpen(player);
		TryToRestore(player);
		chest.item = LootrUtilities.DeepCloneItems(playerItems[player], false);
	}

	public void OnChestClose(Chest chest, int player)
	{
		playerItems[player] = LootrUtilities.DeepCloneItems(chest.item);
		TrySheduleRestore(player);
	}

	private void OnFirstOpen(int player)
	{
		if ((!playerItems.TryGetValue(player, out var items) || items.All(item => item.IsAir)) && !playerRestoreTime.ContainsKey(player))
			playerItems[player] = LootrUtilities.DeepCloneItems(worldGenItems);
	}

	private void TryToRestore(int player)
	{
		if (!playerRestoreTime.TryGetValue(player, out uint timeToRestore)) return;
		var playerTimeInWorld = Main.player[player].GetModPlayer<LootrPlayer.LootrPlayer>().TimeInWorld;
		if (timeToRestore > playerTimeInWorld) return;

		playerRestoreTime.Remove(player);
		playerItems[player] = LootrUtilities.DeepCloneItems(worldGenItems);
	}

	private void TrySheduleRestore(int player)
	{
		if (!LootrConfig.Instance.AllowRestore || playerRestoreTime.ContainsKey(player) || !playerItems[player].All(item => item.IsAir)) return;
		playerRestoreTime[player] = (uint)LootrConfig.Instance.SecondsToRestore * 60;
	}
}
