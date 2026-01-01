using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LootrMod.Config;

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
	[Range(20 * 60, 24 * 3600)]
	[DefaultValue(20 * 60)]
	public int SecondsToRestore;

	[BackgroundColor(0, 0, 0, 0)]
	[ReloadRequired]
	[DefaultValue(false)]
	public bool Debug;
}
