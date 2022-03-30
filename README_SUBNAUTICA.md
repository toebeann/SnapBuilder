# SnapBuilder

## Finally, we can buld perfectly aligned and rotated items!

### Fully customisable!

By default, snapping is enabled with this mod installed. The level of snapping can be fine-tuned in the Mod options menu, as can all keybinds.

## [Check out SnapBuilder in action!](https://youtu.be/xolRjATSr94?t=80)
Big thanks to [Sketchy](https://www.youtube.com/channel/UCDz6lJ5Ba3S_FsI0hpoUMXg) of [Nitrox](https://nitrox.rux.gg) support for the shout out!

[![Check out SnapBuilder in action on YouTube: What's new this week at Subnautica Nexus as of March 11th 2020, episode 1](https://imgur.com/80gYa9a.gif)](https://youtu.be/xolRjATSr94?t=80)

![SnapBuilder 1.4 Feature Preview: Wall snapping](https://imgur.com/Opg6CEE.gif)

## Installating SnapBuilder

### Recommended installation (automated)
1. Install [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager).
2. Click the **Install with Mod Manager** button at the top of the page.
3. Run the game via the Thunderstore Mod Manager.
4. ???
5. Profit.

### Manual installation
1. Make sure you first install all of the dependencies listed above under **This mod requires the following mods to function**, and install their dependencies, and so on...
2. Click the **Manual Download** button at the top of the page.
3. Extract **only** the QMods folder from the archive into `<Steam Location>\steamapps\common\Subnautica`.
4. Check that you have installed it correctly. You should have the SnapBuilder folder at the following location: `<Steam Location>\steamapps\common\Subnautica\QMods\SnapBuilder`.

## Default keybinds
| Action                                 | Keybind        |
| -------------------------------------- | -------------- |
| Toggle snap-to-grid/freehand placement | `middle mouse` |
| Toggle fine grid snapping              | `left ctrl`    |
| Toggle fine rotation snapping          | `left alt`     |

## Need help?
Your first port of call when in need of help with a Subnautica mod should be the [Subnautica Modding discord server](https://discord.gg/UpWuWwq).

Hop into one of their `#❓help-and-support❓` channels where there are tons of really helpful, knowledgeable people who can guide you through troubleshooting configuration or installation issues.

If however you've stumbled on a bug, feel free to [file an issue on GitHub](https://github.com/toebeann/SnapBuilder/issues), but please make sure to browse through the other issues to make sure your bug isn't already listed! Please include as much information as possible, including screenshots if applicable, steps to reproduce the issue, and a detailed description of the bug and what you expected to happen.

## Changelogs
<details open>
  <summary>Version 1.4.1</summary>
  This is a bugfix release.

  * Fixes a `NullReferenceException` that triggers when building the base hatch.
  * Fixes the rotation alignment when SnapBuilder is used in the Cyclops.
  * Increased compatibility with Habitat Platform mod.
</details>

<details open>
  <summary>Version 1.4</summary>
  This is a feature release.

  * New feature: Automatically snap floor-placed items flush with the wall/each other when aiming at the wall or the side of an item. ![SnapBuilder 1.4 Feature Preview: Wall snapping](https://imgur.com/Opg6CEE.gif)
  * New feature: Inaccurate collider meshes are automatically swapped out for better ones so that items can properly snap to the terrain floor. This feature is mostly only useful in the Below Zero version of the mod. ![SnapBuilder 1.4 Feature Preview: Improved colliders](https://imgur.com/Gg0inXT.gif)
  * Added option to disable displaying hints.
  * Misc. improvements to SnapBuilder logic.
</details>

<details>
  <summary>Version 1.3.7.1</summary>
  This is a bugfix release.

  * Bugfix: Using SnapBuilder outdoors or in the cyclops should work as expected.
</details>

<details>
  <summary>Version 1.3.7</summary>
  This is a maintenance release.

  * General improvements to snapping inside a base
</details>

<details>
  <summary>Version 1.3.6</summary>
  This is a maintenance release.

  * Updated for VersionChecker 1.2
</details>

<details>
  <summary>Version 1.3.5</summary>
  
  * Added a `Toggle Rotation` keybind for placeable items so that you don't get locked to an item in the hotbar if it can be rotated.
</details>

<details>
  <summary>Version 1.3.4.1</summary>
  
  * Hotfix for an issue where custom keybinds were causing crashes on game launch
</details>

<details>
  <summary>Version 1.3.4</summary>
  
  * Maintenance update for SMLHelper 2.9
  * Hotfix for `FineSnapRounding` values
</details>

<details>
  <summary>Version 1.3.3</summary>
  
  * Fixes for how objects which can be placed from the hotbar are handled
  * Compatibility enhancements for the Builder Upgrade Module mod
</details>

<details>
  <summary>Version 1.3.2</summary>
  
  * Fixes a bug introduced in v1.2.4 where rebound keys were not initialised correctly when relaunching the game.
</details>

<details>
  <summary>Version 1.3.1</summary>
  
  * Fixed a bug introduced in v1.2.3 where SnapBuilder keybinds no longer worked.
</details>

<details>
  <summary>Version 1.3</summary>
  
  * Added compatibility for Custom Posters or any object which can be placed from the hotbar rather than built via the Habitat Builder!
</details>

<details>
  <summary>Version 1.2.4</summary>
  
  * Added VersionChecker support
</details>

<details>
  <summary>Version 1.2.3</summary>
  
  * Updated for QMM4 and SMLHelper 2.8
</details>

<details>
  <summary>Version 1.2</summary>
  
  * Added SnapBuilder button prompt hints when using the Habitat Builder
  * Added SMLHelper language override support
</details>

<details>
  <summary>Version 1.1.1</summary>
  
  * Bugfix: Missing default options from previous versions will now be correctly set.
</details>

<details>
  <summary>Version 1.1</summary>
  
  * Added ability to customise whether individual keybinds should be held or pressed.
</details>

<details>
  <summary>Version 1.0.2</summary>
  
  * Bugfix: Toggle snapping key can be changed as intended.
</details>

<details>
  <summary>Version 1.0.1</summary>
  
  * Updated to Harmony version 1.2.0.1
  * Reworked rotation to be more consistent across different button types (hold, scroll etc.)
</details>