using LootrMod.Networking;
using LootrMod.Systems;
using Terraria;
using Terraria.ModLoader;

namespace LootrMod.LootrPlayer;

internal class LootrPlayer : ModPlayer
{
	public uint TimeInWorld => Main.GameUpdateCount - joinTimestamp;
	private uint joinTimestamp;
	private int lastChest = -1;
	private int currentChest = -1;

	public override void OnEnterWorld() => joinTimestamp = Main.GameUpdateCount;

	public override void PostUpdate()
	{
		currentChest = (short)Player.chest;
		if (lastChest != currentChest)
		{
			if (lastChest != -1 && currentChest == -1)
				LootrSystem.OnChestClose(lastChest, Player.whoAmI);

			else if (currentChest != -1)
				LootrSystem.OnChestOpen(currentChest, Player.whoAmI);

			lastChest = currentChest;
		}
	}

	//TODO: Make this shit work when world is unloaded (singleplayer)
	public override void PreSavePlayer() => LootrNetwork.SendSubstructTimers(Player.whoAmI, TimeInWorld);
}
