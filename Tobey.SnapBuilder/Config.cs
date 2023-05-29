using BepInEx.Configuration;
using System;
using UnityEngine;

namespace Tobey.SnapBuilder;
public static class Config
{
    internal static ConfigFile Cfg => SnapBuilder.Instance.Config;

    public static class Localisation
    {
        public static void Initialise() { }

        public static ConfigEntry<string> ToggleSnapping { get; } =
                Cfg.Bind(
                    section: nameof(Localisation),
                    key: nameof(ToggleSnapping),
                    defaultValue: "Snapping"
                );

        public static ConfigEntry<string> ToggleFineSnapping { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(ToggleFineSnapping),
                defaultValue: "Fine snapping"
            );

        public static ConfigEntry<string> ToggleRotation { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(ToggleRotation),
                defaultValue: "Rotation"
            );

        public static ConfigEntry<string> ToggleFineRotation { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(ToggleFineRotation),
                defaultValue: "Fine rotation"
            );

        public static ConfigEntry<string> HolsterItem { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(HolsterItem),
                defaultValue: "Holster item"
            );

        public static ConfigEntry<string> DetailedCollider { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(DetailedCollider),
                defaultValue: "Detailed collider"
            );

        public static ConfigEntry<string> OriginalCollider { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(OriginalCollider),
                defaultValue: "Original collider"
            );

        public static ConfigEntry<string> ToggleExtendedBuildRange { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(ToggleExtendedBuildRange),
                defaultValue: "Extended build range");

        public static ConfigEntry<string> AdjustBuildRange { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(AdjustBuildRange),
                defaultValue: "Adjust build range");

        public static ConfigEntry<string> ResetBuildRange { get; } =
            Cfg.Bind(
                section: nameof(Localisation),
                key: nameof(ResetBuildRange),
                defaultValue: "Reset build range");
    }

    public static class General
    {
        internal static void Initialise() { }

        public static ConfigEntry<bool> DisplayControlHints { get; } =
            Cfg.Bind(
                section: nameof(General),
                key: "Display control hints",
                defaultValue: true,
                description: "Whether to display SnapBuilder control hints while operating the Habitat Builder."
            );

        public static ConfigEntry<bool> RenderDetailedColliders { get; } =
            Cfg.Bind(
                section: nameof(General),
                key: "Render detailed colliders",
                defaultValue: true,
                description: "Whether detailed colliders are rendered while using the Habitat Builder."
            );
    }

    public static class Keybinds
    {
        internal static void Initialise() { }

        public static ConfigEntry<KeyboardShortcut> Snapping { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle snapping",
                defaultValue: new KeyboardShortcut(KeyCode.Mouse2),
                description: "Shortcut to toggle snapping."
            );

        public static ConfigEntry<Toggle.ToggleMode> SnappingMode { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle snapping mode",
                defaultValue: Toggle.ToggleMode.Press,
                description: "Whether snapping is toggled on keypress or while holding the shortcut."
            );

        public static ConfigEntry<KeyCode> FineSnapping { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle fine snapping",
                defaultValue: KeyCode.LeftControl,
                configDescription: new(
                    description: "Shortcut to toggle fine snapping.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -10 } }
                )
            );

        public static ConfigEntry<Toggle.ToggleMode> FineSnappingMode { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle fine snapping mode",
                defaultValue: Toggle.ToggleMode.Hold,
                configDescription: new(
                    description: "Whether fine snapping is toggled on keypress or while holding the shortcut.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -10 } }
                )
            );

        public static ConfigEntry<KeyCode> FineRotation { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle fine rotation",
                defaultValue: KeyCode.LeftAlt,
                configDescription: new(
                    description: "Shortcut to toggle fine rotation.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -20 } }
                )
            );

        public static ConfigEntry<Toggle.ToggleMode> FineRotationMode { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle fine rotation mode",
                defaultValue: Toggle.ToggleMode.Hold,
                configDescription: new(
                    description: "Whether fine rotation is toggled on keypress or while holding the shortcut.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -20 } }
                )
            );

        public static ConfigEntry<KeyboardShortcut> Rotation { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle rotation (for placeable items)",
                defaultValue: new KeyboardShortcut(KeyCode.Q),
                configDescription: new(
                    description: "Shortcut to toggle rotation while holding a placeable item.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -30 } }
                )
            );

        public static ConfigEntry<Toggle.ToggleMode> RotationMode { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle rotation mode (for placeable items)",
                defaultValue: Toggle.ToggleMode.Hold,
                configDescription: new(
                    description: "Whether rotation of placeable items is toggled on keypress or while holding the shortcut.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -30 } }
                )
            );

        public static ConfigEntry<KeyboardShortcut> DetailedColliders { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle detailed colliders",
                defaultValue: new KeyboardShortcut(KeyCode.F),
                configDescription: new(
                    description: "Shortcut to toggle detailed colliders.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -40 } }
                )
            );

        public static ConfigEntry<Toggle.ToggleMode> DetailedCollidersMode { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle detailed colliders mode",
                defaultValue: Toggle.ToggleMode.Press,
                configDescription: new(
                    description: "Whether detailed colliders are toggled on keypress or while holding the shortcut.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -40 } }
                )
            );

        public static ConfigEntry<KeyCode> ExtendedBuildRange { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle extended build range",
                defaultValue: KeyCode.B,
                configDescription: new(
                    description: "Shortcut to toggle the extended build range.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -50 } }
                )
            );

        public static ConfigEntry<Toggle.ToggleMode> ExtendedBuildRangeMode { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Toggle extended build range mode",
                defaultValue: Toggle.ToggleMode.Press,
                configDescription: new(
                    description: "Whether the extended build range is toggled on keypress or while holding the key.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -50 } }
                )
            );


        public static ConfigEntry<KeyCode> IncreaseExtendedBuildRange { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Increase extended build range",
                defaultValue: KeyCode.Mouse4,
                configDescription: new(
                    description: "Shortcut to increase the extended build range.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -60 } }
                )
            );
        public static ConfigEntry<KeyCode> DecreaseExtendedBuildRange { get; } =
            Cfg.Bind(
                section: nameof(Keybinds),
                key: "Decrease extended build range",
                defaultValue: KeyCode.Mouse3,
                configDescription: new(
                    description: "Shortcut to decrease the extended build range.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -61 } }
                )
            );
    }

    public static class Snapping
    {
        internal static void Initialise() { }

        public static ConfigEntry<bool> EnabledByDefault { get; } =
            Cfg.Bind(
                section: nameof(Snapping),
                key: "Enable snapping by default",
                defaultValue: true,
                description: "Whether snapping is enabled by default."
            );

        public static ConfigEntry<bool> DetailedCollidersEnabledByDefault { get; } =
            Cfg.Bind(
                section: nameof(Snapping),
                key: "Enable detailed colliders by default",
                defaultValue: true,
                configDescription: new(
                    description: "Whether detailed colliders are enabled by default.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -10 } }
                )
            );

        public static ConfigEntry<int> SnapRounding { get; } =
            Cfg.Bind(
                section: nameof(Snapping),
                key: "Snap rounding",
                defaultValue: 50,
                configDescription: new(
                    description: $"The rounding factor to apply to snapping.{Environment.NewLine}" +
                    $"This determines the size of the grid while snapping is enabled.{Environment.NewLine}" +
                    "Higher values correspond to a larger grid with fewer snap points.",
                    acceptableValues: new AcceptableValueRange<int>(1, 100),
                    tags: new[] {
                        new ConfigurationManagerAttributes
                        {
                            Order = -20,
                            ShowRangeAsPercent = false
                        }
                    }
                )
            );

        public static ConfigEntry<int> FineSnapRounding { get; } =
            Cfg.Bind(
                section: nameof(Snapping),
                key: "Fine snap rounding",
                defaultValue: 20,
                configDescription: new(
                    description: $"The rounding factor to apply to fine snapping.{Environment.NewLine}" +
                    $"This determines the size of the grid while fine snapping is enabled.{Environment.NewLine}" +
                    "Higher values correspond to a larger grid with fewer snap points.",
                    acceptableValues: new AcceptableValueRange<int>(1, 100),
                    tags: new[] {
                        new ConfigurationManagerAttributes
                        {
                            Order = -30,
                            ShowRangeAsPercent = false
                        }
                    }
                )
            );

        public static ConfigEntry<int> RotationRounding { get; } =
            Cfg.Bind(
                section: nameof(Snapping),
                key: "Rotation rounding (degrees)",
                defaultValue: 45,
                configDescription: new(
                    description: "The rounding factor to apply to rotation in degrees.",
                    acceptableValues: new AcceptableValueRange<int>(1, 90),
                    tags: new[] { new ConfigurationManagerAttributes { Order = -40 } }
                )
            );

        public static ConfigEntry<int> FineRotationRounding { get; } =
            Cfg.Bind(
                section: nameof(Snapping),
                key: "Fine rotation rounding (degrees)",
                defaultValue: 5,
                configDescription: new(
                    description: "The rounding factor to apply to fine rotation in degrees.",
                    acceptableValues: new AcceptableValueRange<int>(1, 45),
                    tags: new[] { new ConfigurationManagerAttributes { Order = -50 } }
                )
            );
    }

    public static class ExtendedBuildRange
    {
        public const string Section = "Extended build range";

        internal static void Initialise() { }

        public static ConfigEntry<bool> EnabledByDefault { get; } =
            Cfg.Bind(
                section: Section,
                key: "Enable extended build range by default",
                defaultValue: false,
                description: "Whether the extended build range is enabled by default."
            );

        public static ConfigEntry<float> Multiplier { get; } =
            Cfg.Bind(
                section: Section,
                key: "Build range multiplier",
                defaultValue: 1.5f,
                configDescription: new(
                    description: "The multiplier applied to the build range when using the Habitat Builder with the extended build range.",
                    tags: new[] { new ConfigurationManagerAttributes { Order = -10 } }
                )
            );
    }

    public static class Toggles
    {
        internal static void Initialise() { }

        public static Toggle Snapping { get; } =
            new(
                shortcut: Keybinds.Snapping,
                mode: Keybinds.SnappingMode,
                enabledByDefault: Config.Snapping.EnabledByDefault
            );

        public static Toggle FineSnapping { get; } =
            new(
                keyCode: Keybinds.FineSnapping,
                mode: Keybinds.FineSnappingMode
            );

        public static Toggle FineRotation { get; } =
            new(
                keyCode: Keybinds.FineRotation,
                mode: Keybinds.FineRotationMode
            );

        public static Toggle Rotation { get; } =
            new(
                shortcut: Keybinds.Rotation,
                mode: Keybinds.RotationMode
            );

        public static Toggle DetailedColliders { get; } =
            new(
                shortcut: Keybinds.DetailedColliders,
                mode: Keybinds.DetailedCollidersMode,
                enabledByDefault: Config.Snapping.DetailedCollidersEnabledByDefault
            );

        public static Toggle ExtendBuildRange { get; } =
            new(
                keyCode: Keybinds.ExtendedBuildRange,
                mode: Keybinds.ExtendedBuildRangeMode,
                enabledByDefault: ExtendedBuildRange.EnabledByDefault
            );

        public static void Bind()
        {
            Snapping.Bind();
            FineSnapping.Bind();
            FineRotation.Bind();
            Rotation.Bind();
            DetailedColliders.Bind();
            ExtendBuildRange.Bind();
        }

        public static void Unbind()
        {
            Snapping.Unbind();
            FineSnapping.Unbind();
            FineRotation.Unbind();
            Rotation.Unbind();
            DetailedColliders.Unbind();
            ExtendBuildRange.Unbind();
        }

        public static void Reset()
        {
            Snapping.Reset();
            FineSnapping.Reset();
            FineRotation.Reset();
            Rotation.Reset();
            DetailedColliders.Reset();
            ExtendBuildRange.Reset();
        }

        public static void Dispose()
        {
            Snapping.Dispose();
            FineSnapping.Dispose();
            FineRotation.Dispose();
            Rotation.Dispose();
            DetailedColliders.Dispose();
            ExtendBuildRange.Dispose();
        }
    }
}
