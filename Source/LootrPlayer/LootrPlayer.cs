using LootrMod.Networking;
using LootrMod.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LootrMod.LootrPlayer
{
	internal class LootrPlayer : ModPlayer
	{
		private int lastChest = -1;

		public override void OnEnterWorld() =>
			LootrNetwork.SendDataToPlayer(Player.whoAmI);

		public override void PostUpdate()
		{
			if (lastChest != Player.chest)
			{
				if (lastChest != -1 && Player.chest == -1)
					LootrSystem.OnChestClosed(lastChest, Player.whoAmI);
				
				else if (Player.chest != -1)
					LootrSystem.OnChestOpened(Player.chest, Player.whoAmI);

				lastChest = Player.chest;
			}
		}
	}
}
