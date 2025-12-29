using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
using LootrMod.Networking;
using LootrMod.Utilities;

namespace LootrMod.Systems;

public abstract class UniquePlayerSystem : ModSystem
{
	public override void Load() {
		IL_NetMessage.SyncOnePlayer += RemoveGuidOnPlayerLeft;
		IL_NetMessage.SyncOnePlayer += TestGuidInMultiplayer;
	}
	
	private static void TestGuidInMultiplayer(ILContext il)
	{
		var c = new ILCursor(il);
		var serverOnPlayerLeft = new Func<Instruction, bool>[]
		{
			i => i.MatchLdcI4(19),
			i => i.MatchLdelemRef(),
			i => i.MatchLdcI4(1)
		};
		c.GotoNext(serverOnPlayerLeft);
		c.Index--;
		c.EmitLdarg(0);
		c.EmitDelegate<Action<int>>(player =>
			Console.WriteLine($"Server: {Main.player[player].GetModPlayer<UniquePlayer>().Guid}"));
	}
	
	private static void RemoveGuidOnPlayerLeft(ILContext il)
	{
		var c = new ILCursor(il);
		var serverOnPlayerLeft = new Func<Instruction, bool>[]
		{
			i => i.MatchLdcI4(20),
			i => i.MatchLdelemRef(),
			i => i.MatchLdcI4(1)
		};
		c.GotoNext(serverOnPlayerLeft);
		c.Index--;
		c.EmitLdarg(0);
		c.EmitDelegate<Action<int>>(player => UniqueSystem.RemovePlayer(player));
	}
}

#region Server-Side
public static class UniqueSystem
{
	private static Dictionary<int, Guid> SessionIds { get; } = new(Main.player.Length);

	#region GetGuid
	public static Guid GetGuid(int whoAmI) => Check(() => SessionIds[whoAmI]);

	public static Guid GetGuid(Player player) => GetGuid(player.whoAmI);
	#endregion

	#region HasGuid
	public static bool HasGuid(Guid guid) => Check(guid, () => SessionIds.ContainsValue(guid));

	public static bool HasGuid(string guid) => HasGuid(Guid.Parse(guid));
	#endregion

	#region GetWhoAmI
	public static int GetWhoAmI(Guid guid) => Check(guid, () =>
	{
		var kvp = SessionIds.FirstOrDefault(kvp => kvp.Value == guid);
		return kvp.Value == Guid.Empty ? throw new KeyNotFoundException() : kvp.Key;
	});

	public static int GetWhoAmI(string guid) => GetWhoAmI(Guid.Parse(guid));
	#endregion

	#region HasWhoAmI
	public static bool HasWhoAmI(int whoami) => Check(() => SessionIds.ContainsKey(whoami));

	public static bool HasWhoAmI(Player player) => HasWhoAmI(player.whoAmI);
	#endregion

	#region GetPlayer
	public static Player GetPlayer(Guid guid) => Check(guid, () => Main.player[GetWhoAmI(guid)]);

	public static Player GetPlayer(string guid) => GetPlayer(Guid.Parse(guid));
	#endregion

	#region SetGuid
	internal static void SetGuid(int whoAmI, Guid guid) => Check(guid, () => SessionIds[whoAmI] = guid);

	internal static void SetGuid(int whoAmI, string guid) => SetGuid(whoAmI, Guid.Parse(guid));

	internal static void SetGuid(Player player, Guid guid) => SetGuid(player.whoAmI, guid);

	internal static void SetGuid(Player player, string guid) => SetGuid(player.whoAmI, Guid.Parse(guid));
	#endregion

	#region RemovePlayer
	internal static bool RemovePlayer(int whoAmI) => Check(() => SessionIds.Remove(whoAmI));

	internal static bool RemovePlayer(Player player) => RemovePlayer(player.whoAmI);
	#endregion

	#region Checker
	private static T Check<T>(Func<T> function)
	{
		CheckForServerSide();
		return function();
	}

	private static T Check<T>(Guid guid, Func<T> function)
	{
		CheckForServerSide();
		CheckForEmptyGuid(guid);
		return function();
	}

	private static void CheckForServerSide()
	{
		if (Main.netMode == NetmodeID.SinglePlayer) return;
		if (!Main.dedServ) throw new Exception("Interacting with a server-side field on the client");
	}

	private static void CheckForEmptyGuid(Guid guid)
	{
		if (guid.IsEmpty()) throw new ArgumentException("Guid can't be empty");
	}
	#endregion
}
#endregion

#region Client-Side
public class UniquePlayer : ModPlayer
{
	public Guid Guid { get; private set; }

	public override void SaveData(TagCompound tag)
	{
		if (Guid.IsEmpty()) Guid = Guid.NewGuid();
		tag["lootr_guid"] = Guid.ToString("N");
	}

	public override void LoadData(TagCompound tag)
	{
		if (!tag.ContainsKey("lootr_guid")) return;
		var guid = Guid.Parse(tag.GetString("lootr_guid"));
		if (guid.IsEmpty()) return;
		Guid = guid;
	}

	public override void OnEnterWorld()
	{
		switch (Main.netMode)
		{
			case NetmodeID.SinglePlayer: UniqueSystem.SetGuid(Player, Guid); break;
			case NetmodeID.MultiplayerClient: Player.SendGuidToHandle(Guid); break;
		}
	}
}
#endregion
