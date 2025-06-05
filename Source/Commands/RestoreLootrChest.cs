using System;
using LootrMod.Systems;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace LootrMod.Source.Commands
{
	public class RestoreLootrChest : ModCommand
	{
		public override string Command => "restoreLootrChests";
		public override CommandType Type => CommandType.World;
		public override string Usage => "/restoreLootrChests [playerID] [chestID](optional)";
		public override string Description => "Restores Lootr chest(s) of a player.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length < 1) throw new UsageException("No arguments provided!", Color.Red);
			if (args.Length > 2)  throw new UsageException("Too many arguments!",    Color.Red);

			byte player; short chest = 0;
			try {
				player = Convert.ToByte(args[0]);
				if (args.Length > 1) chest = Convert.ToInt16(args[1]);
			} catch { throw new UsageException("Arguments convertation error!", Color.Red); }

			if (args.Length == 1)
				foreach (var (_, lootrChest) in LootrSystem.lootrChests)
					lootrChest.playerRestoreTime.Remove(player);
			else
			{
				LootrSystem.TryGetLootrChest(chest, out var _, out var lootrChest);
				lootrChest.playerRestoreTime.Remove(player);
			}
			caller.Reply("Chest(s) restored successfully!", Color.Green);
		}
	}
}
