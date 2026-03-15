<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>SLAYER_WeaponZoom</strong></h2>
</div>

<p align="center">
  <img src="https://img.shields.io/badge/build-passing-brightgreen" alt="Build Status">
  <img src="https://img.shields.io/github/downloads/zakriamansoor47/SLAYER_WeaponZoom/total" alt="Downloads">
  <img src="https://img.shields.io/github/stars/zakriamansoor47/SLAYER_WeaponZoom?style=flat&logo=github" alt="Stars">
  <img src="https://img.shields.io/github/license/zakriamansoor47/SLAYER_WeaponZoom" alt="License">
</p>

# Accepting Paid Request! Discord: Slayer47#7002
# Donation
If you like this project, consider supporting me:

<a href="https://www.buymeacoffee.com/slayer47" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>
[![PayPal](https://www.paypalobjects.com/webstatic/mktg/logo/pp_cc_mark_37x23.jpg)](https://paypal.me/zakriamansoor)

## Description
SLAYER_WeaponZoom is a plugin for CS2 that allows players to zoom in with their weapons by holding down the secondary attack button (right-click) without needing to use the weapon's built-in scope functionality. The plugin provides customizable zoom levels for different weapon types, including pistols, rifles, and sniper rifles, allowing players to have more control over their aiming and accuracy while using various weapons in the game. It also includes features such as configurable zoom FOV values, support for different weapon types, and options to enable or disable zoom functionality for specific weapons or player groups. Administrators can also restrict zoom functionality to certain player groups using admin flags, providing flexibility in how the plugin is used on different servers.

## Features
- Customizable zoom levels for different weapon types (pistols, rifles, sniper rifles).
- Configurable zoom FOV values for each weapon type.
- Option to enable or disable zoom functionality for specific weapons or player groups.
- Smooth zooming effect for a more immersive experience.
- Zoom Rate configuration to adjust how quickly the zoom effect is applied.
- Admin flag support to restrict zoom functionality to certain player groups.

## Installation
1. Download the latest release of SLAYER_WeaponZoom from the [Releases](https://github.com/zakriamansoor47/SLAYER_WeaponZoom/releases) page.
2. Extract the downloaded archive to the `plugins` directory of your CS2 server.
3. Restart your CS2 server to load the plugin.
4. Configure the plugin settings by editing the `SLAYER_WeaponZoom.jsonc` file located in the `configs/plugins/SLAYER_WeaponZoom` directory.

## Configuration
The plugin can be configured by editing the `SLAYER_WeaponZoom.jsonc` file. Below are the available configuration options:

```jsonc
{
  "WZ_ZoomEnabledByDefault": false, // Set to true to enable zoom for all players by default, false to disable by default and only allow players with specific admin flags to use zoom
  "WZ_SmoothZoom": true, // Enable or disable smooth zooming effect
  "WZ_ZoomRate": 3, // Adjust how quickly the zoom effect is applied. Higher values will make the zoom effect faster
  "WZ_ZoomButton": "Mouse2", // The button used to activate zoom (default is "Mouse2" for right-click)
  "WZ_AdminFlagToUse": "", // Admin flag(s) to use, separate with commas (,) for multiple flags, leave empty to allow all players to use zoom by default
  "WZ_WeaponsZoom":  // A dictionary where the key is the weapon class name and the value is the FOV to use when zooming with that weapon
  {
    "weapon_awp": 10,
    "weapon_g3sg1": 10,
    "weapon_scar20": 10,
    "weapon_scout": 15,
    "weapon_ak47": 60,
    "weapon_famas": 60,
    "weapon_galilar": 60,
    "weapon_m4a1": 60,
    "weapon_m4a1_silencer": 60,
    "weapon_bizon": 60,
    "weapon_mp7": 60,
    "weapon_mp9": 60,
    "weapon_mac10": 60,
    "weapon_mp5sd": 60,
    "weapon_p90": 60,
    "weapon_ump45": 60,
    "weapon_nova": 60,
    "weapon_xm1014": 60,
    "weapon_sawedoff": 60,
    "weapon_mag7": 90,
    "weapon_m249": 100,
    "weapon_negev": 100,
    "weapon_deagle": 70,
    "weapon_elite": 70,
    "weapon_fiveseven": 70,
    "weapon_glock": 70,
    "weapon_p250": 70,
    "weapon_tec9": 70,
    "weapon_usp_silencer": 70,
    "weapon_hkp2000": 70,
    "weapon_cz75a": 70,
    "weapon_revolver": 70,
    "weapon_taser": 70
  }
}
```

## 🎥 SLAYER Weapon Zoom Demo

[![Weapon Zoom Plugin](https://img.youtube.com/vi/o32yJXNHpvU/maxresdefault.jpg)](https://www.youtube.com/watch?v=o32yJXNHpvU)

Demonstration of the **SLAYER Weapon Zoom plugin for CS2** showing smooth zoom, pistol zoom, and scope weapon behavior.

## Author
- [SLAYER](https://github.com/zakriamansoor47/)