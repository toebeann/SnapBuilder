using System.Collections.Generic;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal static class Lang
    {
        internal static class Hint
        {
            public const string ToggleSnapping = "GhostToggleSnappingHint";
            public const string ToggleFineSnapping = "GhostToggleFineSnappingHint";
            public const string ToggleRotation = "GhostToggleRotationHint";
            public const string ToggleFineRotation = "GhostToggleFineRotationHint";
            public const string HolsterItem = "GhostHolsterItemHint";
            public const string DetailedCollider = "GhostToggleDetailedColliderHint";
        }

        internal static class Option
        {
            public const string DisplayControlHints = "Options.DisplayControlHints";
            public const string SnappingEnabledByDefault = "Options.SnappingEnabledByDefault";
            public const string ToggleSnappingKey = "Options.ToggleSnappingKey";
            public const string ToggleSnappingMode = "Options.ToggleSnappingMode";
            public const string FineSnappingKey = "Options.FineSnappingKey";
            public const string FineSnappingMode = "Options.FineSnappingMode";
            public const string FineRotationKey = "Options.FineRotationKey";
            public const string FineRotationMode = "Options.FineRotationMode";
            public const string ToggleRotationKey = "Options.ToggleRotationKey";
            public const string ToggleRotationMode = "Options.ToggleRotationMode";
            public const string DetailedColliderEnabledByDefault = "Options.DetailedColliderEnabledByDefault";
            public const string DetailedColliderKey = "Options.DetailedColliderKey";
            public const string DetailedColliderMode = "Options.DetailedColliderMode";
            public const string SnapRounding = "Options.SnapRounding";
            public const string FineSnapRounding = "Options.FineSnapRounding";
            public const string RotationRounding = "Options.RotationRounding";
            public const string FineRotationRounding = "Options.FineRotationRounding";
        }

        public static void Initialise()
        {
            SMLHelper.Language.Set(new Dictionary<string, string>()
            {
                [Option.DisplayControlHints] = "Display control hints",
                [Hint.ToggleSnapping] = "Snapping",
                [Hint.ToggleFineSnapping] = "Fine snapping",
                [Hint.ToggleRotation] = "Rotation",
                [Hint.ToggleFineRotation] = "Fine rotation",
                [Hint.HolsterItem] = "Holster item",
                [Hint.DetailedCollider] = "Detailed collider",
                [Option.SnappingEnabledByDefault] = "Snapping enabled by default",
                [Option.ToggleSnappingKey] = "Snapping button",
                [Option.ToggleSnappingMode] = "Snapping mode",
                [Option.FineSnappingKey] = "Fine snapping button",
                [Option.FineSnappingMode] = "Fine snapping mode",
                [Option.FineRotationKey] = "Fine rotation button",
                [Option.FineRotationMode] = "Fine rotation mode",
                [Option.ToggleRotationKey] = "Rotation button (for placeable items)",
                [Option.ToggleRotationMode] = "Rotation mode (for placeable items)",
                [Option.DetailedColliderEnabledByDefault] = "Detailed collider enabled by default",
                [Option.DetailedColliderKey] = "Detailed collider button",
                [Option.DetailedColliderMode] = "Detailed collider mode",
                [Option.SnapRounding] = "Snap rounding",
                [Option.FineSnapRounding] = "Fine snap rounding",
                [Option.RotationRounding] = "Rotation rounding (degrees)",
                [Option.FineRotationRounding] = "Fine rotation rounding (degrees)"
            });
        }
    }
}
