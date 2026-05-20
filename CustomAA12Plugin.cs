using System;
using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using UnityEngine;

namespace CustomAA12
{
    public class CustomAA12Plugin : Plugin
    {
        public override string Name => "CustomAA12";
        public override string Description => "自定义AA12全自动速射霰弹枪插件";
        public override string Author => "Developer";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredApiVersion => new Version(1, 1, 4);

        public static readonly HashSet<ushort> AA12PickupSerials = new HashSet<ushort>();
        public static readonly HashSet<ushort> AA12FirearmSerials = new HashSet<ushort>();
        private readonly Dictionary<Player, int> _pendingAA12Pickups = new Dictionary<Player, int>();
        private readonly HashSet<Player> _playersDroppingAA12 = new HashSet<Player>();

        private Harmony _harmony;

        private static readonly Vector3 NtfSpawnPoint = new Vector3(134.933f, 297.552f, -43.236f);
        private static readonly Vector3 ChaosSpawnPoint = new Vector3(0.614f, 302.5f, -39.853f);
        private const int AAMagSize = 90;

        public override void Enable()
        {
            ServerEvents.WaveRespawned += OnWaveRespawned;
            PlayerEvents.PickingUpItem += OnPickingUpItem;
            PlayerEvents.ChangedItem += OnChangedItem;
            PlayerEvents.ReloadedWeapon += OnReloadedWeapon;
            PlayerEvents.PickedUpItem += OnPickedUpItem;
            PlayerEvents.DroppingItem += OnDroppingItem;
            PlayerEvents.DroppedItem += OnDroppedItem;

            _harmony = new Harmony("com.customaa12.patch");
            _harmony.PatchAll();

            LabApi.Features.Console.Logger.Info("[CustomAA12] 插件已启用。");
        }

        public override void Disable()
        {
            ServerEvents.WaveRespawned -= OnWaveRespawned;
            PlayerEvents.PickingUpItem -= OnPickingUpItem;
            PlayerEvents.ChangedItem -= OnChangedItem;
            PlayerEvents.ReloadedWeapon -= OnReloadedWeapon;
            PlayerEvents.PickedUpItem -= OnPickedUpItem;
            PlayerEvents.DroppingItem -= OnDroppingItem;
            PlayerEvents.DroppedItem -= OnDroppedItem;

            _harmony?.UnpatchAll("com.customaa12.patch");
            _harmony = null;

            AA12PickupSerials.Clear();
            AA12FirearmSerials.Clear();
            _pendingAA12Pickups.Clear();
            _playersDroppingAA12.Clear();
        }

        private void OnWaveRespawned(WaveRespawnedEventArgs ev)
        {
            int spawnCount;
            Vector3 spawnPos;

            if (ev.Wave is MtfWave)
            {
                spawnCount = 3;
                spawnPos = NtfSpawnPoint;
            }
            else if (ev.Wave is MiniMtfWave)
            {
                spawnCount = 5;
                spawnPos = NtfSpawnPoint;
            }
            else if (ev.Wave is ChaosWave)
            {
                spawnCount = 3;
                spawnPos = ChaosSpawnPoint;
            }
            else if (ev.Wave is MiniChaosWave)
            {
                spawnCount = 5;
                spawnPos = ChaosSpawnPoint;
            }
            else
            {
                return;
            }

            SpawnAA12Weapons(spawnCount, spawnPos);
        }

        private void SpawnAA12Weapons(int count, Vector3 position)
        {
            for (int i = 0; i < count; i++)
            {
                Pickup pickup = Pickup.Create(
                    ItemType.GunAK,
                    position,
                    Quaternion.identity,
                    Vector3.one
                );

                pickup.Spawn();
                AA12PickupSerials.Add(pickup.Serial);
            }

            LabApi.Features.Console.Logger.Debug(
                $"[CustomAA12] 在 {position} 生成了 {count} 把 AA12。");
        }

        private void OnPickingUpItem(PlayerPickingUpItemEventArgs ev)
        {
            if (ev.Pickup == null || ev.Player == null)
                return;

            if (!AA12PickupSerials.Contains(ev.Pickup.Serial))
                return;

            AA12PickupSerials.Remove(ev.Pickup.Serial);

            ev.Player.SendBroadcast(
                "你捡起了AA12:一把全自动速射霰弹枪",
                5,
                Broadcast.BroadcastFlags.Normal,
                true
            );

            if (!_pendingAA12Pickups.ContainsKey(ev.Player))
                _pendingAA12Pickups[ev.Player] = 0;
            _pendingAA12Pickups[ev.Player]++;
        }

        private void OnPickedUpItem(PlayerPickedUpItemEventArgs ev)
        {
            if (ev.Player == null || ev.Item == null)
                return;

            TryMatchAA12Firearm(ev.Player);
        }

        private void OnChangedItem(PlayerChangedItemEventArgs ev)
        {
            if (ev.Player == null)
                return;

            TryMatchAA12Firearm(ev.Player);
        }

        private void TryMatchAA12Firearm(Player player)
        {
            if (!_pendingAA12Pickups.TryGetValue(player, out int pending) || pending <= 0)
                return;

            Item currentItem = player.CurrentItem;
            if (currentItem == null || currentItem.Type != ItemType.GunAK)
                return;

            if (!(currentItem is FirearmItem firearm))
                return;

            if (AA12FirearmSerials.Contains(firearm.Serial))
                return;

            AA12FirearmSerials.Add(firearm.Serial);
            _pendingAA12Pickups[player]--;

            firearm.StoredAmmo = AAMagSize;

            LabApi.Features.Console.Logger.Debug(
                $"[CustomAA12] 玩家 {player.Nickname} 的枪械 (Serial: {firearm.Serial}) 已标记为AA12。");
        }

        private void OnReloadedWeapon(PlayerReloadedWeaponEventArgs ev)
        {
            if (ev.FirearmItem == null || ev.Player == null)
                return;

            if (!AA12FirearmSerials.Contains(ev.FirearmItem.Serial))
                return;

            ev.FirearmItem.StoredAmmo = AAMagSize;
        }

        private void OnDroppingItem(PlayerDroppingItemEventArgs ev)
        {
            if (ev.Player == null || ev.Item == null)
                return;

            if (!(ev.Item is FirearmItem firearm))
                return;

            if (!AA12FirearmSerials.Contains(firearm.Serial))
                return;

            AA12FirearmSerials.Remove(firearm.Serial);
            _playersDroppingAA12.Add(ev.Player);
        }

        private void OnDroppedItem(PlayerDroppedItemEventArgs ev)
        {
            if (ev.Player == null || ev.Pickup == null)
                return;

            if (!_playersDroppingAA12.Remove(ev.Player))
                return;

            AA12PickupSerials.Add(ev.Pickup.Serial);
        }
    }

    [HarmonyPatch(typeof(AutomaticActionModule), "get_ChamberSize")]
    public static class ChamberSizePatch
    {
        public static void Postfix(AutomaticActionModule __instance, ref int __result)
        {
            if (__result == 7)
                return;

            var firearm = __instance.Firearm;
            if (firearm == null)
                return;

            if (CustomAA12Plugin.AA12FirearmSerials.Contains(firearm.ItemSerial))
                __result = 7;
        }
    }

    [HarmonyPatch(typeof(HitscanHitregModuleBase), "DamageAtDistance")]
    public static class DamagePatch
    {
        private const float DamageMultiplier = 0.22f;

        public static void Postfix(HitscanHitregModuleBase __instance, ref float __result)
        {
            var firearm = __instance.Firearm;
            if (firearm == null)
                return;

            if (CustomAA12Plugin.AA12FirearmSerials.Contains(firearm.ItemSerial))
                __result *= DamageMultiplier;
        }
    }
}