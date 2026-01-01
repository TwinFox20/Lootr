using System;
using LootrMod.DataStructures;
using LootrMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LootrMod.Commands;

public class CreateLootrChest : ModCommand
{
	public override string Command => "createLootrChest";
	public override CommandType Type => CommandType.World;
	public override string Usage => "/createLootrChest [chestID]";
	public override string Description => "Create a Lootr chest from existing one and write all items in it as world generated.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length < 1) throw new UsageException("No arguments provided!", Color.Red);
		if (args.Length > 1)  throw new UsageException("Too many arguments!", Color.Red);

		var chest = Main.chest[Convert.ToUInt16(args[0])];
		var position = new Point16(chest.x, chest.y);
		LootrSystem.LootrChests[position] = new LootrChest(chest.item) ;
		chest.name = "Lootr Chest";
		caller.Reply("New Lootr chest successfully created!", Color.Green);
	}
}
