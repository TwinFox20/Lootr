using System;
using System.Collections.Generic;
using System.Linq;
using LootrMod.Config;
using LootrMod.Utilities;
using Terraria;
using Terraria.ModLoader.IO;

namespace LootrMod.DataStructures;

public sealed class LootrChest(Item[] baseLoot)
{
    private readonly Item[] _baseLoot = LootrUtilities.DeepCloneItems(baseLoot);
    private Dictionary<Guid, Item[]> PlayerItems { get; } = [];
    public Dictionary<Guid, uint> RestoreTimers { get; } = [];

    public static LootrChest Load(TagCompound tag)
    {
        var baseLoot = LootrUtilities.ReadItems(tag.GetList<TagCompound>("GeneratedItems"));
        var chest = new LootrChest(baseLoot);
        
        foreach (var entry in tag.GetList<TagCompound>("Items"))
        {
            if (!Guid.TryParse(entry.GetString("player"), out var playerId)) continue;
            chest.PlayerItems[playerId] = LootrUtilities.ReadItems(entry.GetList<TagCompound>("items"));
        }
        
        foreach (var entry in tag.GetList<TagCompound>("PlayerRestoreTime"))
        {
            if (!Guid.TryParse(entry.GetString("player"), out var playerId)) continue;
            chest.RestoreTimers[playerId] = entry.Get<uint>("time");
        }
        
        return chest;
    }

    public TagCompound Save()
    {
        return new TagCompound
        {
            ["base_loot"] = LootrUtilities.WriteItems(_baseLoot),
            
            ["player_items"] = PlayerItems.Select(pair => new TagCompound {
                ["player"] = pair.Key.ToString("N"),
                ["items"] = LootrUtilities.WriteItems(pair.Value)
            }).ToList(),
            
            ["restore_timers"] = RestoreTimers.Select(pair => new TagCompound {
                ["player"] = pair.Key.ToString("N"),
                ["time"] = pair.Value
            }).ToList()
        };
    }

    public Item[] GetPlayerItems(Guid playerId) => PlayerItems.TryGetValue(playerId, out var items) ? items : [];
    
    public void HandleChestOpened(Chest chest, Guid playerId)
    {
        EnsurePlayerItems(playerId);
        TryRestoreLoot(playerId);

        chest.item = LootrUtilities.DeepCloneItems(PlayerItems[playerId], false);
    }
    
    public void HandleChestClosed(Chest chest, Guid playerId)
    {
        PlayerItems[playerId] = LootrUtilities.DeepCloneItems(chest.item);
        ScheduleRestoreIfEmpty(playerId);
    }

    private void EnsurePlayerItems(Guid playerId)
    {
        var hasLoot = PlayerItems.TryGetValue(playerId, out var items) && !items.IsAir();
        if (!hasLoot && !RestoreTimers.ContainsKey(playerId)) PlayerItems[playerId] = CloneBaseLoot();
    }

    private void TryRestoreLoot(Guid playerId)
    {
        if (!RestoreTimers.TryGetValue(playerId, out var restoreTime)) return;
        if (restoreTime > 1) return;

        RestoreTimers.Remove(playerId);
        PlayerItems[playerId] = CloneBaseLoot();
    }

    private void ScheduleRestoreIfEmpty(Guid playerId)
    {
        var config = LootrConfig.Instance;
        if (!config.AllowRestore) return;
        if (RestoreTimers.ContainsKey(playerId)) return;
        if (!PlayerItems[playerId].IsAir()) return;

        RestoreTimers[playerId] = (uint)config.SecondsToRestore * 60;
    }

    private Item[] CloneBaseLoot() => LootrUtilities.DeepCloneItems(_baseLoot);
}
