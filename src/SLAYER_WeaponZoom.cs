using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.Translation;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace SLAYER_WeaponZoom;

#pragma warning disable CS9107

public class SLAYER_WeaponZoomConfig
{
    public bool WZ_ZoomEnabledByDefault { get; set; } = true;
    public List<string> WZ_Commands { get; set; } = new List<string> { "wz", "weaponzoom" };
    public bool WZ_ZoomHands { get; set; } = true;
    public bool WZ_SmoothZoom { get; set; } = true;
    public int WZ_ZoomRate { get; set; } = 3; // Higher is faster
    public string WZ_ZoomButton { get; set; } = "Mouse2";
    public string WZ_AdminFlagToUse { get; set; } = ""; // Admin flag(s) to use, seperate with commas (,) for multiple flags, leave empty to allow all players to use zoom by default
    public Dictionary<string, int> WZ_WeaponsZoom { get; set; } = new Dictionary<string, int>
    {
        { "weapon_awp", 10 },
        { "weapon_scar20", 10 },
        { "weapon_g3sg1", 10 },
        { "weapon_ssg08", 15 },
        { "weapon_ak47", 60 },
        { "weapon_m4a1", 60 },
        { "weapon_m4a1_silencer", 60 },
        { "weapon_famas", 60 },
        { "weapon_galil", 60 },
        { "weapon_m249", 60 },
        { "weapon_negev", 60 },
        { "weapon_xm1014", 60 },
        { "weapon_sawedoff", 60 },
        { "weapon_nova", 60 },
        { "weapon_mag7", 60 },
        { "weapon_p90", 60 },
        { "weapon_bizon", 60 },
        { "weapon_mp5sd", 60 },
        { "weapon_mp7", 60 },
        { "weapon_mp9", 60 },
        { "weapon_ump45", 60 },
        { "weapon_mac10", 60 },
        { "weapon_deagle", 70 },
        { "weapon_elite", 70 },
        { "weapon_fiveseven", 70 },
        { "weapon_glock", 70 },
        { "weapon_hkp2000", 70 },
        { "weapon_p250", 70 },
        { "weapon_tec9", 70 },
        { "weapon_usp_silencer", 70 },
        { "weapon_revolver", 70 },
        { "weapon_cz75a", 70 },
        { "weapon_taser", 70 }
    };
}
[PluginMetadata
(
    Id = "SLAYER_WeaponZoom", 
    Version = "1.0", 
    Name = "SLAYER_WeaponZoom", 
    Author = "SLAYER", 
    Description = "Allows players to zoom their weapons"
)]
public partial class SLAYER_WeaponZoom(ISwiftlyCore core) : BasePlugin(core)
{
    private sealed class PlayerZoomState
    {
        public bool ZoomEnabled { get; set; } = false;
        public uint DefaultFov { get; set; } = 90;
        public uint CurrentFov { get; set; } = 90;
        public string LastWeaponName { get; set; } = string.Empty;
        public CBasePlayerWeapon? LastWeapon { get; set; } = null!;
        public int LastButtonPressedTick { get; set; } = 0;
        public CancellationTokenSource? ZoomTimerCancellation { get; set; } = null;
    }

    public static new ISwiftlyCore Core { get; private set; } = null!;
    private ServiceProvider? _provider;
    private SLAYER_WeaponZoomConfig Config { get; set; } = new();
    private ILocalizer Localizer => core.Localizer;    
    private readonly Dictionary<IPlayer, PlayerZoomState> _playerZoomStates = new();
    public List<string> Pistols = new List<string> { "weapon_deagle", "weapon_elite", "weapon_fiveseven", "weapon_glock", "weapon_hkp2000", "weapon_p250", "weapon_tec9", "weapon_usp_silencer", "weapon_revolver", "weapon_cz75a", "weapon_taser" };
    public List<string> ScopeWeapons = new List<string> { "weapon_awp", "weapon_scar20", "weapon_g3sg1", "weapon_ssg08" , "weapon_aug", "weapon_sg556" };
    public List<IPlayer> _allValidPlayers = new List<IPlayer>();
    int _playerCacheUpdateTick = 0;
    public override void Load(bool hotReload) 
    {
        // Ensure static Core is initialized before any usage.
        // Swiftly injects the instance core via the primary constructor parameter `core`.
        Core = core;

        // Initialize configuration
        Core.Configuration.InitializeJsonWithModel<SLAYER_WeaponZoomConfig>("SLAYER_WeaponZoom.jsonc", "Main")
        .Configure(builder =>
        {
            builder.AddJsonFile("SLAYER_WeaponZoom.jsonc", optional: false, reloadOnChange: true);
        });

        // Register configuration with dependency injection
        ServiceCollection services = new();
        services.AddSwiftly(Core).AddOptionsWithValidateOnStart<SLAYER_WeaponZoomConfig>().BindConfiguration("Main");

        _provider = services.BuildServiceProvider();

        Config = _provider.GetRequiredService<IOptions<SLAYER_WeaponZoomConfig>>().Value;


        foreach(var cmd in Config.WZ_Commands)
        {
            Core.Command.RegisterCommand(cmd, (CommandInfo) =>
            {
                var player = CommandInfo.Sender;
                if (player == null || !player.IsValid || player.PlayerPawn == null || player.PlayerPawn.LifeState != (int)LifeState_t.LIFE_ALIVE) return;

                var permissions = Config.WZ_AdminFlagToUse.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
                if(!string.IsNullOrEmpty(Config.WZ_AdminFlagToUse) && permissions.Count > 0 && !Core.Permission.PlayerHasPermissions(player.SteamID, permissions))
                {
                    player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.NoPermission"]}");
                    return;
                }
                var zoomState = GetOrCreatePlayerZoomState(player);
                _playerZoomStates[player].ZoomEnabled = !zoomState.ZoomEnabled;
                if (!zoomState.ZoomEnabled)
                {
                    if (zoomState.CurrentFov < zoomState.DefaultFov) DisableZoom(player, true); // Instantly disable zoom when toggling off to prevent FOV issues, will be re-enabled on the next tick if the player has a valid zoom weapon equipped
                    if(zoomState.LastWeapon != null && zoomState.LastWeapon.IsValid)
                    {
                        zoomState.LastWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount; // Reset secondary attack cooldown when zoom is disabled to prevent it from being stuck on cooldown if the player toggles zoom off while right-clicking with a weapon that has secondary attack functionality
                        zoomState.LastWeapon = null!;
                    }
                    var activeWeapon = player.PlayerPawn!.WeaponServices!.ActiveWeapon.Value;
                    if (activeWeapon != null && activeWeapon.IsValid) activeWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount; 
                }  
                player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.ToggleMessage", zoomState.ZoomEnabled]}");
            });
        }
        Core.Event.OnTick += () =>
        {
            _playerCacheUpdateTick++;
            if (_playerCacheUpdateTick >= 24) // Update player cache every 24 ticks (approximately every 1.2 seconds)
            {
                _allValidPlayers = Core.PlayerManager.GetAllValidPlayers().Where(p => p.Controller.TeamNum > 1 && !p.IsFakeClient && !p.Controller.IsHLTV && p.PlayerPawn != null && p.PlayerPawn.LifeState == (int)LifeState_t.LIFE_ALIVE).ToList();
                _playerCacheUpdateTick = 0;
            }

            if (_allValidPlayers.Count == 0) return;
            foreach (var player in _allValidPlayers)
            {
                if (!player.IsValid || player.PlayerPawn == null || player.PlayerPawn.LifeState != (int)LifeState_t.LIFE_ALIVE || player.PlayerPawn.WeaponServices == null) continue;
                var pawn = player.PlayerPawn;
                var zoomState = GetOrCreatePlayerZoomState(player);

                if (!zoomState.ZoomEnabled) continue;

                var activeWeapon = pawn.WeaponServices.ActiveWeapon.Value;
                if (activeWeapon == null || !activeWeapon.IsValid) continue;

                uint ZoomFov = 60; // Default zoom FOV
                bool isPistol = false;
                bool isScopeWeapon = false;
                var weaponName = string.Empty;
                var Vdata = activeWeapon.VData.As<CCSWeaponBaseVData>();
                var baseWeapon = activeWeapon.As<CCSWeaponBaseGun>();
                if (!string.IsNullOrEmpty(activeWeapon.DesignerName))
                {
                    weaponName = Core.Helpers.GetClassnameByDefinitionIndex(activeWeapon.AttributeManager.Item.ItemDefinitionIndex) ?? activeWeapon.DesignerName.ToLower();
                    if(string.IsNullOrEmpty(zoomState.LastWeaponName)) zoomState.LastWeaponName = weaponName; // Initialize last weapon reference
                    if(zoomState.LastWeapon == null || !zoomState.LastWeapon.IsValid) zoomState.LastWeapon = activeWeapon; // Initialize last weapon reference
                    if(weaponName != zoomState.LastWeaponName) 
                    {
                        zoomState.ZoomTimerCancellation?.Cancel();
                        DisableZoom(player, true); // Instantly disable zoom when switching weapons to prevent FOV issues
                        if(zoomState.LastWeapon != null && activeWeapon != zoomState.LastWeapon)
                        {
                            zoomState.LastWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount;
                            zoomState.LastWeapon = activeWeapon; 
                            zoomState.LastWeaponName = weaponName; 
                        } 
                    }

                    isPistol = Pistols.Contains(weaponName);
                    isScopeWeapon = ScopeWeapons.Contains(weaponName);

                    if(isPistol && Config.WZ_ZoomButton != "Mouse2") isPistol = false;
                    if(isScopeWeapon && Config.WZ_ZoomButton != "Mouse2") isScopeWeapon = false;

                    if (Config.WZ_WeaponsZoom.TryGetValue(weaponName, out var weaponZoomFov)) ZoomFov = (uint)weaponZoomFov;
                    else continue; // If the weapon is not in the zoom config, skip it
                    ZoomFov = Math.Clamp(ZoomFov, 1, zoomState.DefaultFov); // Ensure zoom FOV is always valid
                }

                var ZoomButton = ParseButtonByName(Config.WZ_ZoomButton);
                if (ZoomButton == GameButtonFlags.None) continue; // If the zoom button is invalid, skip processing

                if(ZoomButton == GameButtonFlags.Mouse2)
                {
                    if(!isScopeWeapon) activeWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount + 10000; // Set secondary attack cooldown
                    else
                    {
                        if(!pawn.IsScoped) activeWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount - 1; // Remove secondary attack cooldown for scope weapons to allow scope one time
                        else activeWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount + 10000; // Set secondary attack cooldown when already scoped to prevent scope again
                    }
                }
                
                // Apply Zoom for Pistols and Scope weapons on Mouse2 ButtonDoublePressed to prevent issues with shooting while zooming since we continuously remove the Mouse2 button from the player's pressed buttons while zooming
                if(ZoomButton == GameButtonFlags.Mouse2 && pawn.MovementServices!.ButtonDoublePressed == (ulong)GameButtonFlags.Mouse2)
                {
                    if(!isScopeWeapon) EnableZoom(player, ZoomFov);
                    else EnableZoom(player, ZoomFov, false, (uint)Vdata.ZoomFOV1);
                    if(!isScopeWeapon && !Vdata.HasBurstMode && !Vdata.AllowBurstHolster) activeWeapon.NextSecondaryAttackTick.Value = Core.Engine.GlobalVars.TickCount;
                    continue;
                }
                else if (player.PressedButtons.HasFlag(ZoomButton)) // Right-click by default, can be changed in the config
                {
                    zoomState.LastButtonPressedTick = Core.Engine.GlobalVars.TickCount; // Update the last button pressed tick to prevent zoom from being disabled by the timer while the zoom button is held down
                    if (!isPistol && !isScopeWeapon && Vdata.WeaponType != CSWeaponType.WEAPONTYPE_SHOTGUN) EnableZoom(player, ZoomFov);
                    else
                    {
                        zoomState.ZoomTimerCancellation?.Cancel(); // Cancel any existing zoom timer to prevent multiple timers running simultaneously
                        zoomState.ZoomTimerCancellation = Core.Scheduler.Repeat(1, () =>
                        {
                            pawn.MovementServices!.Buttons.ButtonPressed &= ~ZoomButton; // Continuously remove the zoom button from the player's pressed buttons to prevent not being able to shoot while zooming, this is needed for pistols to allow right-click while zooming without interrupting the zoom, Note: it will Cause ButtonDoublePressed, and that's where we apply the zoom
                        });
                    }
                }
                else
                {
                    if (zoomState.CurrentFov < zoomState.DefaultFov) // Smoothly zoom back out when the zoom button is released.
                    {
                        DisableZoom(player, false);
                        zoomState.ZoomTimerCancellation?.Cancel(); // Cancel any existing zoom timer to prevent conflicts when releasing the zoom button
                        if(pawn.IsScoped && Vdata.WeaponType == CSWeaponType.WEAPONTYPE_SNIPER_RIFLE)
                        {
                            player.ExecuteCommand($"slot3"); // switch to last weapon
                            Core.Scheduler.Delay(1,()=> player.ExecuteCommand($"slot1")); // then switch back to the current weapon to reset the weapon's accuracy penalty and zoom level values which can get stuck if the player releases the zoom button while the weapon is still scoped since the zoom is handled by a timer that continuously removes the zoom button from the player's pressed buttons while zooming instead of relying on the weapon's secondary attack functionality which can cause issues with certain weapons that have long secondary attack cooldowns or no secondary attack functionality at all, this forces the game to update the weapon's state and reset any stuck accuracy penalty or zoom level values when unscoping by simulating a quick weapon switch which also allows the player to maintain their current ammo count and other weapon state values without any issues since it's just switching between the currently equipped weapon and itself in a way that forces the game to update the weapon's state without actually changing anything about the player's loadout or inventory since it's just switching to the same weapon slot twice
                            
                        } 
                    }
                }

            }
            
        };
        Core.GameEvent.HookPost<EventPlayerDisconnect>((@event) =>
        {
            var player = @event.UserIdPlayer;
            if (player == null || !player.IsValid) return HookResult.Continue;

            if (_playerZoomStates.TryGetValue(player, out _)) _playerZoomStates.Remove(player);

            return HookResult.Continue;
        });
        Core.GameEvent.HookPost<EventPlayerSpawn>((@event) =>
        {
            var player = @event.UserIdPlayer;
            if (player == null || !player.IsValid) return HookResult.Continue;

            if (_playerZoomStates.TryGetValue(player, out _)) _playerZoomStates.Remove(player);

            return HookResult.Continue;
        });
    }

    public override void Unload()
    {
        foreach (var player in _allValidPlayers)
        {
            if (player.IsValid && _playerZoomStates.TryGetValue(player, out var zoomState) && zoomState.CurrentFov <= zoomState.DefaultFov)
            {
                DisableZoom(player);
            }
        }

    }
    private void EnableZoom(IPlayer player, uint ZoomFov, bool ZoomInstantly = false, uint StartZoomFovOverride = 0)
    {
        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid || pawn.CameraServices == null)
        {
            return;
        }

        var zoomState = GetOrCreatePlayerZoomState(player);
        var cameraServices = pawn.CameraServices as CCSPlayerBase_CameraServices;

        var Zoom = zoomState.CurrentFov;if (StartZoomFovOverride > 0 && Zoom > StartZoomFovOverride) Zoom = StartZoomFovOverride; // Use the override FOV if provided, this is used for scope weapons to start the zoom at the correct FOV instead of smoothly zooming in from the player's default FOV which can cause issues with certain weapons that have a large difference between their default FOV and zoom FOV since it can take a long time to smoothly zoom in to the correct FOV, this allows scope weapons to instantly set the zoom FOV when right-clicking to prevent any issues with the zoom animation while still allowing smooth zooming for non-scope weapons that don't have such a large difference between their default FOV and zoom FOV since it generally looks better with smooth zooming for those weapons
        if (Zoom == ZoomFov) return; // If the current zoom is already at the desired zoom FOV, do nothing
        if(Config.WZ_SmoothZoom && !ZoomInstantly)
        {
            Zoom -= (uint)Config.WZ_ZoomRate;
        }
        else Zoom = ZoomFov; // Instantly set FOV to zoom FOV if smooth zoom is disabled

        Zoom = Math.Clamp(Zoom, ZoomFov, zoomState.DefaultFov); // Clamp zoom to prevent overshooting when smoothly zooming in
        if(!Config.WZ_ZoomHands) player.Controller.DesiredFOV = Zoom;
        if (cameraServices != null)
        {
            cameraServices.ViewEntity.Raw = uint.MaxValue; // Set view entity to the custom camera prop for zooming, fallback to player camera if something goes wrong
            cameraServices.FOV = Zoom;
        }
        
        zoomState.CurrentFov = Zoom;
        player.Controller.DesiredFOVUpdated();
        pawn.CameraServices.FOVUpdated();
        pawn.CameraServices.ViewEntityUpdated();
        pawn.CameraServicesUpdated();
    }

    private void DisableZoom(IPlayer player, bool ZoomInstantly = false)
    {
        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid || pawn.CameraServices == null)
        {
            return;
        }

        var zoomState = GetOrCreatePlayerZoomState(player);
        var cameraServices = pawn.CameraServices as CCSPlayerBase_CameraServices;
        var defaultFov = zoomState.DefaultFov;
        if (defaultFov <= 0) defaultFov = 90U;

        var Zoom = zoomState.CurrentFov;
        if (Zoom == defaultFov) return; // If already at default FOV, no need to update anything
        if(Config.WZ_SmoothZoom && !ZoomInstantly)
        {
            Zoom += (uint)Config.WZ_ZoomRate;
        }
        else Zoom = defaultFov; // Instantly set FOV to default FOV if smooth zoom is disabled

        Zoom = Math.Clamp(Zoom, 1U, defaultFov); // Clamp zoom to prevent overshooting when smoothly zooming out
        if(!Config.WZ_ZoomHands) player.Controller.DesiredFOV = Zoom;
        if (cameraServices != null)
        {
            cameraServices.ViewEntity.Raw = uint.MaxValue;
            cameraServices.FOV = Zoom;
        }

        zoomState.CurrentFov = Zoom;
        player.Controller.DesiredFOVUpdated();
        pawn.CameraServices.FOVUpdated();
        pawn.CameraServices.ViewEntityUpdated();
        pawn.CameraServicesUpdated();
    }

    private PlayerZoomState GetOrCreatePlayerZoomState(IPlayer player)
    {
        if (_playerZoomStates.TryGetValue(player, out var zoomState))
        {
            return zoomState;
        }

        var defaultFov = player.Controller.DesiredFOV > 0 ? player.Controller.DesiredFOV : 90U;
        var permissions = Config.WZ_AdminFlagToUse.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
        var zoomEnabledByDefault = Config.WZ_ZoomEnabledByDefault;
        if(!string.IsNullOrEmpty(Config.WZ_AdminFlagToUse) && permissions.Count > 0 && !Core.Permission.PlayerHasPermissions(player.SteamID, permissions))
        {
            zoomEnabledByDefault = false;
        }
        zoomState = new PlayerZoomState
        {
            ZoomEnabled = zoomEnabledByDefault,
            DefaultFov = defaultFov,
            CurrentFov = defaultFov
        };

        _playerZoomStates[player] = zoomState;
        return zoomState;
    }

    private GameButtonFlags ParseButtonByName(string buttonName)
    {
        if (Enum.TryParse<GameButtonFlags>(buttonName, true, out var button))
        {
            return button;
        }

        return GameButtonFlags.None;
    }
} 