<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>SLAYER_WeaponZoom</strong></h2>
  <h3>SLAYER_WeaponZoom</h3>
</div>

<p align="center">
  <img src="https://img.shields.io/badge/build-passing-brightgreen" alt="Build Status">
  <img src="https://img.shields.io/github/downloads/SLAYER/SLAYER_WeaponZoom/total" alt="Downloads">
  <img src="https://img.shields.io/github/stars/SLAYER/SLAYER_WeaponZoom?style=flat&logo=github" alt="Stars">
  <img src="https://img.shields.io/github/license/SLAYER/SLAYER_WeaponZoom" alt="License">
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
1. Download the latest release of SLAYER_WeaponZoom from the [Releases](https://github.com/SLAYER/SLAYER_WeaponZoom/releases) page.
2. Extract the downloaded archive to the `plugins` directory of your CS2 server.
3. Restart your CS2 server to load the plugin.
4. Configure the plugin settings by editing the `SLAYER_WeaponZoom.jsonc` file located in the `configs/plugins/SLAYER_WeaponZoom` directory.

## Configuration
The plugin can be configured by editing the `SLAYER_WeaponZoom.jsonc` file. Below are the available configuration options:
- `WZ_SmoothZoom`: (bool) Enable or disable smooth zooming effect. Default is `true`.
- `WZ_ZoomRate`: (int) Adjust how quickly the zoom effect is applied. Higher values will make the zoom effect faster. Default is `3`.
- `WZ_ZoomButton`: (string) The button used to activate zoom (default is "Mouse2" for right-click).
- `WZ_AdminFlagToUse`: (string) Admin flag(s) to use, separate with commas (,) for multiple flags, leave empty to allow all players to use zoom by default.
- `WZ_WeaponsZoom`: (dictionary) A dictionary where the key is the weapon class name and the value is the FOV to use when zooming with that weapon. Example:
  ```json
  {
    "weapon_ak47": 15,
    "weapon_m4a1": 12,
    "weapon_awp": 10
  }
  ```

## Author
- SLAYER