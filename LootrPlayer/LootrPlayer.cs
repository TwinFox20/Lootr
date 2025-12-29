using LootrMod.Systems;
using Terraria.ModLoader;

namespace LootrMod.LootrPlayer;

internal class LootrPlayer : ModPlayer
{
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
}
