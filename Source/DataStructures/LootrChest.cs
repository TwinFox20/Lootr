using System.Collections.Generic;
using System.Linq;
using LootrMod.Config;
using LootrMod.Utilities;
using Terraria;
using Terraria.ModLoader.IO;

namespace LootrMod.DataStructures
{
	public class LootrChest
	{
		public Item[] worldGenItems = [];
		public Dictionary<int, Item[]> playerItems = [];
		public Dictionary<int, ulong> playerRestoreTime = [];

		public TagCompound Save()
		{
			var tag = new TagCompound
			{
				["worldGenItems"] = LootrUtilities.WriteItems(worldGenItems),
				["playerItems"] = playerItems.Select(pair => new TagCompound
				{
					["player"] = pair.Key,
					["items"] = LootrUtilities.WriteItems(pair.Value)
				}),

				["playerRestoreTime"] = playerRestoreTime.Select(pair => new TagCompound
				{
					["player"] = pair.Key,
					["time"] = pair.Value
				})
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
				chest.playerRestoreTime[player] = entry.Get<ulong>("time");
			}

			return chest;
		}

		public void FillChestWithPlayerItems(int player, Chest chest)
		{
			TryRestorePlayerItems(player);
			chest.item = LootrUtilities.DeepCloneItems(playerItems[player]);
		}

		public void SavePlayerItems(int player, Item[] items)
		{
			playerItems[player] = LootrUtilities.DeepCloneItems(items);
			TrySheduleRestore(player);
		}

		private void TryRestorePlayerItems(int player)
		{
			bool shouldRestore = playerRestoreTime.TryGetValue(player, out ulong restoreTime) && restoreTime <= Main.GameUpdateCount;

			if (shouldRestore)
				playerRestoreTime.Remove(player);

			if (!playerItems.ContainsKey(player) || shouldRestore)
				playerItems[player] = LootrUtilities.DeepCloneItems(worldGenItems);
		}

		private void TrySheduleRestore(int player)
		{
			if (!LootrConfig.Instance.AllowRestore)
				return;

			if (playerRestoreTime.ContainsKey(player))
				return;

			if (playerItems[player].All(item => item.IsAir))
				return;

			playerRestoreTime[player] = Main.GameUpdateCount + (ulong)(LootrConfig.Instance.SecondsToRestore * 60);
		}
	}
}
