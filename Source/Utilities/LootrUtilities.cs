using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace LootrMod.Utilities
{
	internal class LootrUtilities
	{
		public static Item[] DeepCloneItems(Item[] items)
		{
			List<Item> copy = [];
			for (int i = 0; i < items.Length; i++)
			{
				bool isAir = items[i].IsAir;
				if (!isAir)
				{
					copy.Add(items[i].Clone());
				}
			}
			return [.. copy];
		}

		public static List<TagCompound> WriteItems(Item[] items)
		{
			List<TagCompound> list = [];
			foreach (Item item in items)
			{
				list.Add(ItemIO.Save(item));
			}
			return list;
		}

		public static Item[] ReadItems(IList<TagCompound> tags)
		{
			Item[] items = new Item[tags.Count];
			for (int i = 0; i < tags.Count; i++)
			{
				items[i] = ItemIO.Load(tags[i]);
			}
			return items;
		}
	}
}
