![SnapBuilder - Snap-to-Grid for Subnautica!](https://staticdelivery.nexusmods.com/mods/1155/images/427/427-1671005676-1635506048.png)

# SnapBuilder - Snap-to-Grid for Subnautica & Subnautica: Below Zero!

## What is this?

SnapBuilder is **the** snap-to-grid mod for Subnautica & Subnautica: Below Zero you've been dreaming of!

### Features

-   Snap-to-grid enabled for all items! (Doesn't apply to rooms as they already snap together)
-   [When aiming at a wall or the side of an item, will automatically snap the item into position!](https://i.imgur.com/kY9Xefg.mp4)
-   [Automatically upgrades inaccurate hitboxes so that items can properly snap to them!](https://i.imgur.com/D8DycbH.mp4) (Mostly only applies to Subnautica: Below Zero)
-   The range for building can be extended and customised! (On the legacy branch of Subnautica, this only applies to items, not rooms)
-   All keybinds and parameters can be tweaked in-game with [Configuration Manager](https://www.nexusmods.com/subnautica/mods/1112/)! (Simply press F5 to open the configuration window)
-   Fully compatible with the QMods and the legacy branch of Subnautica!
-   Fully compatible with [Decorations Mod](https://www.nexusmods.com/subnautica/mods/102)!

![Configuring SnapBuilder](https://staticdelivery.nexusmods.com/mods/1155/images/427/427-1671005686-1718616155.png)

## Installation

### Prerequisites (install these first)

#### **Required**

-   The relevant BepInEx pack for your game:
    -   [**BepInEx pack for Subnautica**](https://www.nexusmods.com/subnautica/mods/1108)
    -   [**BepInEx pack for Subnautica: Below Zero**](https://www.nexusmods.com/subnauticabelowzero/mods/344)

#### Highly recommended

-   [**Vortex Mod Manager**](https://www.nexusmods.com/about/vortex/) (Windows only) - Makes installing mods a breeze.
-   [**BepInEx Tweaks**](https://www.nexusmods.com/subnautica/mods/1104) - Without this, SnapBuilder and many other BepInEx plugins will be disabled whenever you back out to the main menu and will not work again until you restart the game.
-   [**Configuration Manager**](https://www.nexusmods.com/subnautica/mods/1112) - Allows you to configure your settings for this and many other BepInEx plugins in-game.

### Installating SnapBuilder

#### Automatic (Windows only)

1. Get [Vortex Mod Manager](https://www.nexusmods.com/about/vortex/)
2. Click the `Vortex` button at the top of the relevant Nexus Mods page for this mod to install:
    - [SnapBuilder for Subnautica](https://www.nexusmods.com/subnautica/mods/427)
    - [SnapBuilder for Subnautica: Below zero](https://www.nexusmods.com/subnauticabelowzero/mods/57)

#### Manual

1. Click the `Manual Download` button under the main file of the Files tab of the relevant Nexus Mods page to download:
    - [SnapBuilder for Subnautica](https://www.nexusmods.com/subnautica/mods/427?tab=files)
    - [SnapBuilder for Subnautica: Below zero](https://www.nexusmods.com/subnauticabelowzero/mods/57?tab=files)
2. Extract the contents of the archive into the `[game dir]\BepInEx` folder

## Default controls

The following controls are entirely configurable in-game with [Configuration Manager](https://www.nexusmods.com/subnautica/mods/1112/). Simply press F5 to open the configuration window.

-   Toggle snap-to-grid/freehand placement: `middle mouse`
-   Toggle fine grid snapping: `left ctrl`
-   Toggle fine rotation snapping: `left alt`
-   Toggle detailed colliders: `f`
-   Toggle extended build range: `b`
-   Increase extended build range: `mouse4`
-   Decresae extended build range: `mouse3`
-   Reset extended build range: `mouse4` + `mouse3`

## Need help?

Most issues are resolved by carefully re-reading the installation instructions or stickies at the top of the Posts tab on the relevant Nexus Mods page ([Subnautica](https://www.nexusmods.com/subnautica/mods/427?tab=posts), [Below Zero](https://www.nexusmods.com/subnauticabelowzero/mods/57?tab=posts)), but if you have stumbled on a bug, please file a bug report on the Bugs tab ([Subnautica](https://www.nexusmods.com/subnautica/mods/427?tab=bugs), [Below Zero](https://www.nexusmods.com/subnauticabelowzero/mods/57?tab=bugs)) with as much information as possible to help me find the cause and get it squashed in an update.

## I'm a dev, how do I make my mod compatible?

By default, most items added to the game by other mods should work just fine with SnapBuilder assuming they also work fine in-game. For the edge cases where they do not and SnapBuilder does not understand which way to rotate your items by default, it is easy to make your items compatible without a dependency on SnapBuilder.

On your prefab's model `GameObject`, simply add a child `GameObject` named `SnapBuilder`. Whatever you set the `localRotation` and `localPosition` of this `GameObject` to, SnapBuilder will treat these as the default transformations for the item.

The `GameObject` named `SnapBuilder` must be the direct child of the `GameObject` located at `Builder.ghostModel` when the user is interacting with the Habitat Builder.

For example:

```cs
        public override GameObject GetGameObject()
        {
            GameObject prefab = ... // code to load your prefab goes here

            // Get model
            GameObject model = prefab.FindChild("MY_MODEL'S_TRANSFORM");

            // help snapbuilder understand the model
            var snapBuilder = new GameObject("SnapBuilder");
            snapBuilder.transform.SetParent(model.transform, false);
            snapBuilder.transform.localEulerAngles = new Vector3(-90, -90, 0);

            // do other stuff...
        }
```

## Check out these other mods by Tobey!

### [Fast Loading Screen](https://www.nexusmods.com/subnautica/mods/763)

[![Fast Loading Screen](https://staticdelivery.nexusmods.com/mods/2706/images/thumbnails/171/171-1621479562-543452583.jpeg)](https://www.nexusmods.com/subnautica/mods/763)

Turboboost your Subnautica loading times!
