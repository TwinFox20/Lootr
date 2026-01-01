using System;
using System.Collections.Generic;
using System.Linq;
using LootrMod.Config;
using LootrMod.Utilities;
using Terraria;
using Terraria.ModLoader.IO;

namespace LootrMod.DataStructures;

public class LootrChest(Item[] baseLoot)
{
	private readonly Item[] _baseLoot = LootrUtilities.DeepCloneItems(baseLoot);
	private Dictionary<Guid, Item[]> PlayerItems { get; } = [];
	public Dictionary<Guid, uint> RestoreTimers { get; } = [];

	#region NBT
	public static LootrChest Load(TagCompound tag)
	{
		var baseLoot = LootrUtilities.ReadItems(tag.GetList<TagCompound>("baseLoot"));
		var chest = new LootrChest(baseLoot);
		foreach (var entry in tag.GetList<TagCompound>("playerItems"))
		{
			var guid = Guid.Parse(entry.GetString("player"));
			var items = entry.GetList<TagCompound>("items");
			chest.PlayerItems[guid] = LootrUtilities.ReadItems(items);
		}

		foreach (var entry in tag.GetList<TagCompound>("restoreTimers"))
		{
			var guid = Guid.Parse(entry.GetString("player"));
			chest.RestoreTimers[guid] = entry.Get<uint>("time");
		}
		return chest;
	}

	public TagCompound Save()
	{
		return new TagCompound {
			["baseLoot"] = LootrUtilities.WriteItems(_baseLoot),

			["playerItems"] = PlayerItems.Select(pair => new TagCompound {
				["player"] = pair.Key.ToString("N"),
				["items"] = LootrUtilities.WriteItems(pair.Value)
			}).ToList(),

			["restoreTimers"] = RestoreTimers.Select(pair => new TagCompound {
				["player"] = pair.Key.ToString("N"),
				["time"] = pair.Value
			}).ToList()
		};
	}
	#endregion

	public void HandleChestOpened(Chest chest, Guid guid)
	{
		EnsurePlayerItems(guid);
		TryRestoreLoot(guid);
		chest.item = LootrUtilities.DeepCloneItems(PlayerItems[guid], false);
	}

	private void EnsurePlayerItems(Guid guid)
	{
		var empty = !PlayerItems.TryGetValue(guid, out var items) || items.IsAir();
		if (LootrConfig.Instance.Debug)
			Console.WriteLine($"Player {guid}: {(empty ? "don't have loot" : "have loot")}.");
		if (empty && !RestoreTimers.ContainsKey(guid)) PlayerItems[guid] = CloneBaseLoot();
	}

	private void TryRestoreLoot(Guid guid)
	{
		if (!RestoreTimers.TryGetValue(guid, out var restoreTime ) || restoreTime > 1) return;
		RestoreTimers.Remove(guid);
		PlayerItems[guid] = CloneBaseLoot();
	}

	public void HandleChestClosed(Chest chest, Guid guid)
	{
		PlayerItems[guid] = LootrUtilities.DeepCloneItems(chest.item);
		var config = LootrConfig.Instance;
		if (!config.AllowRestore || RestoreTimers.ContainsKey(guid) || !PlayerItems[guid].IsAir()) return;
		RestoreTimers[guid] = (uint)config.SecondsToRestore * 60;
	}

	private Item[] CloneBaseLoot() => LootrUtilities.DeepCloneItems(_baseLoot);
}
