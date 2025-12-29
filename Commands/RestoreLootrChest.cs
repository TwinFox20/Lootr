using System;
using LootrMod.Systems;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace LootrMod.Commands
{
	public class RestoreLootrChest : ModCommand
	{
		public override string Command => "restoreLootrChests";
		public override CommandType Type => CommandType.World;
		public override string Usage => "/restoreLootrChests [playerID] [chestID](optional)";
		public override string Description => "Restores Lootr chest(s) of a player.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			switch (args.Length)
			{
				case < 1: throw new UsageException("No arguments provided!", Color.Red);
				case > 2: throw new UsageException("Too many arguments!", Color.Red);
			}

			Guid player; short chest = 0;
			try {
				player = UniqueSystem.GetGuid(Convert.ToByte(args[0]));
				if (args.Length > 1) chest = Convert.ToInt16(args[1]);
			} catch { throw new UsageException("Arguments conversion error!", Color.Red); }

			if (args.Length == 1)
				foreach (var (_, lootrChest) in LootrSystem.LootrChests)
					lootrChest.RestoreTimers.Remove(player);
			else
			{
				LootrSystem.TryGetLootrChest(chest, out _, out var lootrChest);
				lootrChest?.RestoreTimers.Remove(player);
			}
			caller.Reply("Chest(s) restored successfully!", Color.Green);
		}
	}
}
