using System;
using System.Collections.Generic;
using Humanizer;
using LootrMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace LootrMod.Commands;

public class ShowGuidList : ModCommand
{
	public override string Command => "guidlist";
	public override CommandType Type => CommandType.Console;
	public override string Usage => "/guidlist";
	public override string Description => "Displays a list of current player's guid.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length > 1)  throw new UsageException("Too many arguments!", Color.Red);

		List<Guid> list = [];
		foreach (var player in Main.ActivePlayers) list.Add(UniquePlayerLib.GetGuid(player.whoAmI));
		caller.Reply(list.Humanize(), Color.White);
	}
}
