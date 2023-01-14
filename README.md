![SnapBuilder - Snap-to-Grid for Subnautica!](https://staticdelivery.nexusmods.com/mods/1155/images/427/427-1671005676-1635506048.png)

# SnapBuilder - Snap-to-Grid for Subnautica!

## ATTENTION

**The dependencies and the installation process of SnapBuilder have changed.**

From SnapBuilder 2.0 onwards, SnapBuilder will no longer be released as a mod for [QModManager](https://www.nexusmods.com/subnautica/mods/201). Moving forward, SnapBuilder will be released as a [BepInEx](https://www.nexusmods.com/subnautica/mods/1108) plugin.

This change is to facilitate compatibility with the Subnautica 2.0 Living Large update.

## What is this?

**Finally, we can build perfectly aligned and rotated items with the Habitat Builder!**

Fully customisable - all keybinds and snapping parameters can be tweaked in-game with [Configuration Manager](https://www.nexusmods.com/subnautica/mods/1112/). Simply press F5 to open the configuration window.

By default, snapping is enabled with the mod installed. The level of snapping can be fine-tuned in-game.

![Configuring SnapBuilder](https://staticdelivery.nexusmods.com/mods/1155/images/427/427-1671005686-1718616155.png)

## Manual installation

1. [Install the latest version of BepInEx](https://www.nexusmods.com/subnautica/mods/1108).
2. [Install BepInEx Tweaks](https://www.nexusmods.com/subnautica/mods/1104?tab=files).
3. For in-game configuration, [install Configuration Manager](https://www.nexusmods.com/subnautica/mods/1112/).
4. [Download SnapBuilder from Nexus Mods](https://www.nexusmods.com/subnautica/mods/427?tab=files) and extract the contents of the archive into the `[game dir]\BepInEx` folder.

## Default controls

The following controls are entirely configurable in-game with [Configuration Manager](https://www.nexusmods.com/subnautica/mods/1112/). Simply press F5 to open the configuration window.

-   Toggle snap-to-grid/freehand placement: `middle mouse`
-   Toggle fine grid snapping: `left ctrl`
-   Toggle fine rotation snapping: `left alt`

## I'm a dev, how do I make my mod compatible?

By default, most items added to the game by other mods should work just fine with SnapBuilder assuming they also work fine in-game. For the edge cases where they do not and SnapBuilder does not understand which way to rotate your items by default, it is easy to make your items compatible without requiring your users to install SnapBuilder.

On your prefab's model `GameObject`, simply add a child `GameObject` named `SnapBuilder`. Whatever you set the `localRotation` and `localPosition` of this `GameObject` to, SnapBuilder will treat these as the default translations for the item.

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

## Need help?
Your first port of call when in need of help with a Subnautica mod should be the [Subnautica Modding discord server](https://discord.gg/UpWuWwq). Hop into one of the `#help-and-support` channels where you can find many helpful, knowledgeable people who can guide you through configuration and installation issues.

If however you've stumbled on a bug, please file an issue here on GitHub, or on the [Nexus Mods bugs tab](https://www.nexusmods.com/subnautica/mods/427?tab=bugs) with as much information as possible to help me find the cause and get it squashed in an update.
