using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;

namespace LootrMod.Utilities;

internal static class LootrUtilities
{
	/// <summary>
	/// This function makes a copy of a list of <see cref="Item"/>s using tModLoader’s custom function <see cref="Item.Clone"/>.
	/// </summary>
	/// <param name="items">List of items</param>
	/// <param name="compact">Flag which remove air items</param>
	public static Item[] DeepCloneItems(IList<Item> items, bool compact = true)
	{
		var length = compact ? items.Count : Chest.maxItems;
		var result = new List<Item>(length);
		for (var i = 0; i < length; i++)
		{
			if (i < items.Count && !items[i].IsAir)
				result.Add(items[i].Clone());

			else if (!compact)
				result.Add(new Item());
		}
		return [.. result];
	}

	public static List<TagCompound> WriteItems(IList<Item> items)
	{
		var length = items.Count;
		var result = new TagCompound[length];
		for (var i = 0; i < length; i++) result[i] = ItemIO.Save(items[i]);
		return [.. result];
	}

	public static Item[] ReadItems(IList<TagCompound> tags)
	{
		var length = tags.Count;
		var result = new Item[length];
		for (var i = 0; i < length; i++) result[i] = ItemIO.Load(tags[i]);
		return result;
	}

	public static bool IsAir(this IList<Item> items) =>
		items == null || items.All(i => i.IsAir);

	public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;

	public static string NetModeString()
	{
		return Main.netMode switch
		{
			0 => "singleplayer",
			1 => "multiplayer",
			2 => "server",
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
