using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using InventorySystem.Items;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;
using System.Collections.Concurrent;
using Exiled.Events.EventArgs.Server;

namespace OverwatchSystem
{
    public class Overwatch
    {
        private static readonly ConcurrentDictionary<Player, Player> ObservedPlayers = new();
        private static readonly ConcurrentDictionary<Player, DynamicHint> ActiveHints = new();

        public static void Register()
        {
            try
            {
                Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += OnSpectating;
                Exiled.Events.Handlers.Player.Destroying += OnPlayerLeft;
                Exiled.Events.Handlers.Player.ChangingRole += OnRoleChanged;
                Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
                Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestarting;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to register Overwatch events: {ex}");
            }
        }

        public static void Unregister()
        {
            try
            {
                Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= OnSpectating;
                Exiled.Events.Handlers.Player.Destroying -= OnPlayerLeft;
                Exiled.Events.Handlers.Player.ChangingRole -= OnRoleChanged;
                Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
                Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestarting;

                CleanupState();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to unregister Overwatch events: {ex}");
            }
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            CleanupState();
        }

        private static void OnRoundRestarting()
        {
            CleanupState();
        }

        private static void CleanupState()
        {
            try
            {
                foreach (var hint in ActiveHints.Values)
                {
                    try
                    {
                        if (hint != null)
                        {
                            foreach (var player in Player.List)
                            {
                                if (player != null && player.IsConnected)
                                {
                                    PlayerDisplay.Get(player)?.RemoveHint(hint);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error while cleaning up hint: {ex}");
                    }
                }

                ObservedPlayers.Clear();
                ActiveHints.Clear();
            }
            catch (Exception ex)
            {
                Log.Error($"Error during state cleanup: {ex}");
            }
        }

        private static void OnSpectating(ChangingSpectatedPlayerEventArgs ev)
        {
            try
            {
                if (ev?.Player == null || ev?.NewTarget == null)
                {
                    Log.Debug("OnSpectating: Player or target is null");
                    return;
                }

                if (ev.NewTarget.Role.Type == RoleTypeId.None)
                {
                    Log.Debug("OnSpectating: Target role is None");
                    return;
                }

                if (ev.Player.Role.Type != RoleTypeId.Overwatch)
                {
                    Log.Debug("OnSpectating: Player is not in Overwatch role");
                    return;
                }

                ObservedPlayers[ev.Player] = ev.NewTarget;

                if (ActiveHints.TryGetValue(ev.Player, out var existingHint))
                {
                    try
                    {
                        PlayerDisplay.Get(ev.Player)?.RemoveHint(existingHint);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error removing existing hint: {ex}");
                    }
                }

                DynamicHint hint = new DynamicHint();
                ActiveHints[ev.Player] = hint;
                
                try
                {
                    PlayerDisplay.Get(ev.Player)?.AddHint(hint);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error adding new hint: {ex}");
                    return;
                }

                Timing.RunCoroutine(ShowOverwatch(ev.Player, hint, ev.Player));
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnSpectating: {ex}");
            }
        }

        private static void OnPlayerLeft(DestroyingEventArgs ev)
        {
            try
            {
                if (ev?.Player == null)
                {
                    return;
                }

                RemoveHint(ev.Player);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnPlayerLeft: {ex}");
            }
        }

        private static void OnRoleChanged(ChangingRoleEventArgs ev)
        {
            try
            {
                if (ev?.Player == null)
                {
                    return;
                }

                if (ev.NewRole != RoleTypeId.Overwatch)
                {
                    RemoveHint(ev.Player);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnRoleChanged: {ex}");
            }
        }

        private static void RemoveHint(Player player)
        {
            try
            {
                if (player == null)
                {
                    return;
                }

                if (ActiveHints.TryRemove(player, out var hint))
                {
                    try
                    {
                        PlayerDisplay.Get(player)?.RemoveHint(hint);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error removing hint: {ex}");
                    }
                }

                ObservedPlayers.TryRemove(player, out _);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in RemoveHint: {ex}");
            }
        }

        private static IEnumerator<float> ShowOverwatch(Player player, DynamicHint hint, Player perms)
        {
            if (player == null || hint == null || perms == null)
            {
                yield break;
            }

            var translations = Plugin.Instance.Translation;

            while (player.IsConnected && player.Role.Type == RoleTypeId.Overwatch)
            {
                try
                {
                    if (ObservedPlayers.TryGetValue(player, out var target))
                    {
                        if (target == null)
                        {
                            continue;
                        }

                        if (perms.RemoteAdminAccess)
                        {
                            int Id = target.Id;
                            string nickname = target.Nickname;
                            string nickname_rp = string.IsNullOrEmpty(target.CustomName) ? translations.None : target.CustomName;
                            string cinfo = string.IsNullOrEmpty(target.CustomInfo) ? translations.None : target.CustomInfo;
                            string custom_rola = translations.None;
                            if (target.TryGetSummonedInstance(out SummonedCustomRole role))
                            {
                                custom_rola = role.Role.Name;
                            }
                            RoleTypeId rola = target.Role.Type;
                            
                            Color rola_color = target.Role.Color;
                            string rola_color_hex = $"#{(byte)(rola_color.r * 255):X2}{(byte)(rola_color.g * 255):X2}{(byte)(rola_color.b * 255):X2}";
                            
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"<align=left><color=#4682B4><b>{translations.OverwatchSystem}</b></color>\n");
                            sb.AppendLine($"<color=#D3D3D3>👤</color><color=#A9A9A9>{translations.Nickname}:</color> <b><color=#FFFFFF>{nickname}</color></b>");
                            if (!string.IsNullOrWhiteSpace(nickname_rp) && nickname_rp != translations.None && nickname_rp != nickname)
                                sb.AppendLine($"<color=#D8BFD8>🎭</color><color=#A9A9A9>{translations.Roleplay}:</color> <color=#F0F0F0>{nickname_rp}</color>");
                            if (!string.IsNullOrWhiteSpace(cinfo) && cinfo != translations.None)
                                sb.AppendLine($"<color=#A9A9A9>🔎</color><color=#A9A9A9>{translations.CustomInfo}:</color> <color=#F0F0F0>{cinfo}</color>");
                            sb.AppendLine($" <color=#FFFFE0>🆔</color><color=#A9A9A9>{translations.Id}:</color> <color=#FFFFFF>{Id}</color>");
                            sb.AppendLine($"<color=#FFFFE0>\ud83d\udc68</color><color=#A9A9A9>{translations.CustomRole}:</color> <color=#FFFFFF>{custom_rola}</color>");
                            sb.AppendLine($" <color={rola_color_hex}>⚜️ {translations.Role}: {rola}</color>");
                            sb.AppendLine($"🎽<color=#A9A9A9>{translations.Inventory}:</color>");
                            ShowInventory(target, sb);
                            sb.AppendLine("</align>");

                            hint.Text = sb.ToString();
                            hint.TargetX = 50;
                            hint.TargetY = 950;
                            hint.FontSize = Math.Max(18, 27 - (sb.Length / 100));
                        }
                        else
                        {
                            hint.Text = $"<align=left><color=#FF0000><b>{translations.NoPermissions}</b></color>\n<color=#FFFFFF>{translations.NoPermissionsDescription}</color></align>";
                            hint.TargetX = 50;
                            hint.TargetY = 950;
                            hint.FontSize = 20;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error in ShowWarns loop: {ex}");
                }

                yield return Timing.WaitForSeconds(1f);
            }

            try
            {
                RemoveHint(player);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ShowWarns cleanup: {ex}");
            }
        }

        public static void ShowInventory(Player player, StringBuilder sb)
        {
            try
            {
                if (player == null)
                {
                    return;
                }

                if (sb == null)
                {
                    return;
                }

                var translations = Plugin.Instance.Translation;

                if (player.Items == null || player.Items.Count == 0)
                {
                    sb.AppendLine($"      <color=#808080><i>{translations.Empty}</i></color>");
                    return;
                }

                foreach (var item in player.Items)
                {
                    try
                    {
                        if (item == null)
                            continue;

                        string icon = "📦";
                        switch (item.Type)
                        {
                            case ItemType.GunCOM15:
                            case ItemType.GunE11SR:
                            case ItemType.GunLogicer:
                            case ItemType.GunA7:
                            case ItemType.GunCOM18:
                            case ItemType.GunAK:
                            case ItemType.GunCrossvec:
                            case ItemType.GunRevolver:
                            case ItemType.GunFRMG0:
                            case ItemType.GunFSP9:
                            case ItemType.GunShotgun:
                            case ItemType.GunCom45:
                                icon = "🔫";
                                break;
                            case ItemType.KeycardScientist:
                            case ItemType.KeycardChaosInsurgency:
                            case ItemType.KeycardGuard:
                            case ItemType.KeycardContainmentEngineer:
                            case ItemType.KeycardFacilityManager:
                            case ItemType.KeycardMTFCaptain:
                            case ItemType.KeycardMTFOperative:
                            case ItemType.KeycardMTFPrivate:
                            case ItemType.KeycardO5:
                            case ItemType.KeycardJanitor:
                            case ItemType.KeycardZoneManager:
                            case ItemType.KeycardResearchCoordinator:
                                icon = "🔑";
                                break;
                            case ItemType.ArmorLight:
                            case ItemType.ArmorCombat:
                            case ItemType.ArmorHeavy:
                                icon = "🎽";
                                break;
                            case ItemType.Medkit:
                            case ItemType.Adrenaline:
                            case ItemType.Painkillers:
                                icon = "💉";
                                break;
                            case ItemType.SCP018:
                            case ItemType.SCP207:
                            case ItemType.SCP244a:
                            case ItemType.SCP244b:
                            case ItemType.SCP268:
                            case ItemType.SCP330:
                            case ItemType.SCP1576:
                            case ItemType.SCP1853:
                            case ItemType.SCP2176:
                            case ItemType.AntiSCP207:
                            case ItemType.SCP1344:
                                icon = "🔬";
                                break;
                            case ItemType.MicroHID:
                            case ItemType.ParticleDisruptor:
                                icon = "⚡";
                                break;
                            case ItemType.GrenadeHE:
                            case ItemType.GrenadeFlash:
                                icon = "💣";
                                break;
                            case ItemType.Radio:
                                icon = "📻";
                                break;
                            case ItemType.Flashlight:
                                icon = "🔦";
                                break;
                            case ItemType.Jailbird:
                                icon = "🔨";
                                break;
                            case ItemType.SCP500:
                                icon = "💊";
                                break;
                        }

                        if (CustomItem.TryGet(item, out CustomItem customItem))
                        {
                            string customName = customItem.Name;
                            string customIcon = icon;

                            if (customItem is ICustomItemInfoProvider provider && !string.IsNullOrEmpty(provider.CustomIcon))
                            {
                                customIcon = provider.CustomIcon;
                            }
                            sb.AppendLine($"    {customIcon} {customName} <color=#C0C0C0>{GetCustomItemAdditionalInfo(customItem, item)}</color>");
                        }
                        else
                        {
                            string vanillaNameWithDetails = GetItemNameWithDetails(item);
                            sb.AppendLine($"    {icon} {vanillaNameWithDetails}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing inventory item: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ShowInventory: {ex}");
            }
        }

        private static string GetCustomItemAdditionalInfo(CustomItem customItem, Item item)
        {
            if (customItem == null) return "";
            
            if (customItem is ICustomItemInfoProvider provider)
            {
                string info = provider.AdditionalInfo;
                return info;
            }

            return "";
        }

        private static string GetItemNameWithDetails(Item item)
        {
            if (item == null) return Plugin.Instance.Translation.None;

            string baseName = item.Type.GetName();

            if (item is Firearm firearm)
            {
                int Ammo = firearm.MagazineAmmo + firearm.BarrelAmmo;
                int Max_Ammo = firearm.MaxMagazineAmmo + firearm.MaxBarrelAmmo;
                
                return $"{baseName} <color=#C0C0C0>({Ammo}/{Max_Ammo})</color>";
            }
            if (item is MicroHid microHid)
            {
                float energy = microHid.Energy;
                int energyPercent = Mathf.RoundToInt(energy * 100f);
                
                return $"{baseName} <color=#C0C0C0>({energyPercent}%)</color>";
            }
            if (item is Jailbird jailbird)
            {
                float charges = 5 - jailbird.TotalCharges;
                return $"{baseName} <color=#C0C0C0>({charges})</color>";
            }
            if (item is Radio radio)
            {
                float battery = radio.BatteryLevel;
                
                return $"{baseName} <color=#C0C0C0>({battery}%)</color>";
            }
            return baseName;
        }
    }
}