using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LootrMod.Config
{
	[BackgroundColor(0, 0, 0, 100)]
	public class LootrConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static LootrConfig Instance;

		[Header("Main")]
		[BackgroundColor(0, 0, 0, 0)]
		[DefaultValue(true)]
		public bool AllowRestore;

		[BackgroundColor(0, 0, 0, 0)]
		[Range(0, 86400)]
		[DefaultValue(1200)]
		public int SecondsToRestore;
	}
}
