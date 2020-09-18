using System.Collections.Generic;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal static class Lang
    {
        internal static class Hint
        {
            public const string TOGGLE_SNAPPING = "GhostToggleSnappingHint";
            public const string TOGGLE_FINE_SNAPPING = "GhostToggleFineSnappingHint";
            public const string TOGGLE_ROTATION = "GhostToggleRotationHint";
            public const string TOGGLE_FINE_ROTATION = "GhostToggleFineRotationHint";
            public const string HOLSTER_ITEM = "GhostHolsterItemHint";
        }

        internal static class Option
        {
            public const string DEFAULT_SNAPPING_ENABLED = "Options.SnappingEnabledByDefault";
            public const string TOGGLE_SNAPPING_KEY = "Options.ToggleSnappingKey";
            public const string TOGGLE_SNAPPING_MODE = "Options.ToggleSnappingMode";
            public const string FINE_SNAPPING_KEY = "Options.FineSnappingKey";
            public const string FINE_SNAPPING_MODE = "Options.FineSnappingMode";
            public const string FINE_ROTATION_KEY = "Options.FineRotationKey";
            public const string FINE_ROTATION_MODE = "Options.FineRotationMode";
            public const string TOGGLE_ROTATION_KEY = "Options.ToggleRotationKey";
            public const string TOGGLE_ROTATION_MODE = "Options.ToggleRotationMode";
            public const string SNAP_ROUNDING = "Options.SnapRounding";
            public const string FINE_SNAP_ROUNDING = "Options.FineSnapRounding";
            public const string ROTATION_ROUNDING = "Options.RotationRounding";
            public const string FINE_ROTATION_ROUNDING = "Options.FineRotationRounding";
        }

        public static void Initialise()
        {
            SMLHelper.Language.Set(new Dictionary<string, string>()
            {
                [Hint.TOGGLE_SNAPPING] = "Toggle snapping",
                [Hint.TOGGLE_FINE_SNAPPING] = "Toggle fine snapping",
                [Hint.TOGGLE_ROTATION] = "Toggle rotation",
                [Hint.TOGGLE_FINE_ROTATION] = "Toggle fine rotation",
                [Hint.HOLSTER_ITEM] = "Holster item",
                [Option.DEFAULT_SNAPPING_ENABLED] = "Snapping enabled by default",
                [Option.TOGGLE_SNAPPING_KEY] = "Toggle snapping button",
                [Option.TOGGLE_SNAPPING_MODE] = "Toggle snapping mode",
                [Option.FINE_SNAPPING_KEY] = "Fine snapping button",
                [Option.FINE_SNAPPING_MODE] = "Fine snapping mode",
                [Option.FINE_ROTATION_KEY] = "Fine rotation button",
                [Option.FINE_ROTATION_MODE] = "Fine rotation mode",
                [Option.TOGGLE_ROTATION_KEY] = "Toggle rotation button (for placeable items)",
                [Option.TOGGLE_ROTATION_MODE] = "Toggle rotation mode (for placeable items)",
                [Option.SNAP_ROUNDING] = "Snap rounding",
                [Option.FINE_SNAP_ROUNDING] = "Fine snap rounding",
                [Option.ROTATION_ROUNDING] = "Rotation rounding (degrees)",
                [Option.FINE_ROTATION_ROUNDING] = "Fine rotation rounding (degrees)"
            });
        }
    }
}
