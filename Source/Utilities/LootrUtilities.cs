using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace LootrMod.Utilities;

internal class LootrUtilities
{
	/// <summary>
	/// Using <see cref="Item.Clone"/> on each item and remove air<br/>
	/// Set <paramref name="compact"/> to false to fill with air
	/// </summary>
	/// <param name="items">Array </param>
	/// <param name="compact">Flag whitch remove air items</param>
	public static Item[] DeepCloneItems(IList<Item> items, bool compact = true)
	{
		var length = compact ? items.Count : 40;
		var result = new List<Item>(length);
		for (byte i = 0; i < length; i++)
			if (i < items.Count && !items[i].IsAir)
				result.Add(items[i].Clone());
			else if (!compact)
				result.Add(new Item());
		return [.. result];
	}

	public static List<TagCompound> WriteItems(IList<Item> items)
	{
		var length = items.Count;
		var result = new TagCompound[length];
		for (byte i = 0; i < length; i++)
			result[i] = ItemIO.Save(items[i]);
		return [.. result];
	}

	public static Item[] ReadItems(IList<TagCompound> tags)
	{
		var length = tags.Count;
		var result = new Item[length];
		for (byte i = 0; i < length; i++)
			result[i] = ItemIO.Load(tags[i]);
		return result;
	}
}
