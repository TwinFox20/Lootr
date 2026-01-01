using System;
using System.Collections.Generic;
using LootrMod.Networking;
using LootrMod.Systems;
using LootrMod.Utilities;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LootrMod.LootrPlayer;

public class LootrPlayer : ModPlayer
{
	public static Dictionary<Point16, uint> RestoreTimers = [];

	private int _lastChest = -1;
	private int _currentChest = -1;

	public override void PostUpdate()
	{
		_currentChest = (short)Player.chest;
		if (_lastChest == _currentChest) return;
		if (_lastChest != -1 && _currentChest == -1)
			LootrSystem.HandleChestClosed(_lastChest, Player.whoAmI);
		else if (_currentChest != -1)
			LootrSystem.HandleChestOpened(_currentChest, Player.whoAmI);
		_lastChest = _currentChest;
	}

	public override void PlayerDisconnect() {
		Console.WriteLine($"Player {Player.whoAmI} disconnected as {LootrUtilities.NetModeString()}");
		Player.SendSubtractTimers(100);
	}
}
