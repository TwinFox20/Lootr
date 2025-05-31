using System;
using System.Linq;
using Humanizer;
using LootrMod.Config;
using LootrMod.DataStructures;
using LootrMod.Networking;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace LootrMod.LootrPlayer;

internal class LootrPlayer : ModPlayer
{
	private short lastChest = -1;
	private short currentChest = -1;

	public override void OnEnterWorld() => LootrNetwork.SendDataToPlayer(Player.whoAmI);

	public override void PostUpdate()
	{
		currentChest = (short)Player.chest;
		if (lastChest != currentChest)
		{
			if (lastChest != -1 && Player.chest == -1)
				LootrSystem.OnChestClosed(lastChest, Player.whoAmI);
			
			else if (Player.chest != -1)
				LootrSystem.OnChestOpened(currentChest, Player.whoAmI);

			lastChest = currentChest;
		}
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (!LootrConfig.Instance.DebugMode) return;

		if (LootrMod.DisplayFieldsKeybind.JustReleased)
		{
			Main.NewText("Chest: " + currentChest);
			if (currentChest < 0) return;

			LootrSystem.TryGetLootrChest(lastChest, out _, out var lootrChest);
			if (lootrChest == null) return;

			var player = Player.whoAmI;

			if (!lootrChest.playerItems.TryGetValue(player, out var playeritems)) return;
			Main.NewText($"Player items: {playeritems.Select(i => i.Name).Humanize()}");

			if (!lootrChest.playerRestoreTime.TryGetValue(player, out var restoretime)) return;
			//FIXME: change Main.GameUpdateTime on sth better!!!
			var timeSpan = TimeSpan.FromSeconds((restoretime - Main.GameUpdateCount) / 60);
			Main.NewText($"Remaining time to restore: {restoretime / 60} {timeSpan:hh\\:mm\\:ss}");
		}

		if (LootrMod.CreateLootrChestKeybind.JustReleased)
		{
			Main.NewText("Chest: " + currentChest);
			if (currentChest < 0) return;

			var chest = Main.chest[currentChest];
			var position = new Point16(chest.x, chest.y);
			LootrSystem.lootrChests[position] = new LootrChest
			{
				worldGenItems = LootrUtilities.DeepCloneItems(chest.item)
			};
			chest.name = "Lootr Chest";
		}
	}
}
