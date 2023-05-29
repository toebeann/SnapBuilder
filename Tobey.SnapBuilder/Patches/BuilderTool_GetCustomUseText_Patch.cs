using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace Tobey.SnapBuilder.Patches;
using static Config;
internal static class BuilderTool_GetCustomUseText_Patch
{
    public struct GetCustomUseTextState
    {
        public bool WasPlacing;
        public bool WasPlacingRotatable;
        public bool WasSnappingEnabled;
        public bool WereHintsEnabled;
        public bool WasColliderImprovable;
        public bool WasColliderImproved;
    }

    private static bool IsSnappingEnabled => Toggles.Snapping.IsEnabled;
    private static bool AreHintsEnabled => General.DisplayControlHints.Value;
    private static bool IsColliderImprovable => AimTransform.Instance?.ColliderCache?.Record?.IsImprovable ?? false;
    private static bool IsColliderImproved => AimTransform.Instance?.ColliderCache?.Record?.IsImproved ?? false;

    private static bool wasSnappingEnabled = Toggles.Snapping.IsEnabled;
    private static bool wereHintsEnabled = AreHintsEnabled;
    private static bool wasColliderImprovable = false;
    private static bool wasColliderImproved = false;

    [HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.GetCustomUseText))]
    [HarmonyPrefix, HarmonyWrapSafe]
    public static void GetCustomUseTextPrefix(object __instance, out GetCustomUseTextState __state)
    {
        var builderTool = Traverse.Create(__instance);

        __state = new GetCustomUseTextState
        {
            WasPlacing = builderTool.Field("wasPlacing").GetValue<bool>(),
            WasPlacingRotatable = builderTool.Field("wasPlacingRotatable").GetValue<bool>(),
            WasSnappingEnabled = wasSnappingEnabled,
            WereHintsEnabled = wereHintsEnabled,
            WasColliderImprovable = wasColliderImprovable,
            WasColliderImproved = wasColliderImproved
        };

        wasSnappingEnabled = Toggles.Snapping.IsEnabled;
        wereHintsEnabled = General.DisplayControlHints.Value;
        wasColliderImprovable = IsColliderImprovable;
        wasColliderImproved = IsColliderImproved;
    }

    [HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.GetCustomUseText))]
    [HarmonyPostfix, HarmonyWrapSafe]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "I'm using them locally.")]
    public static string GetCustomUseTextPostfix(string customUseText, object __instance, GetCustomUseTextState __state)
    {
        var builderTool = Traverse.Create(__instance);
        var updateCustomUseTextMethod = builderTool.Method("UpdateCustomUseText");
        var customUseTextField = builderTool.Field("customUseText");
        var isPlacingBasePiece = Builder.prefab?.GetComponent<ConstructableBase>() != null;

        if (Builder.isPlacing)
        {
            if (!AreHintsEnabled && __state.WereHintsEnabled)
            {
                updateCustomUseTextMethod.GetValue();
                return customUseTextField.GetValue<string>();
            }
            else if (AreHintsEnabled
                     && (!__state.WereHintsEnabled
                         || !__state.WasPlacing
                         || IsSnappingEnabled != __state.WasSnappingEnabled
                         || IsColliderImprovable != __state.WasColliderImprovable
                         || IsColliderImproved != __state.WasColliderImproved
                         || Builder.rotationEnabled != __state.WasPlacingRotatable))
            {
                updateCustomUseTextMethod.GetValue();
                customUseText = customUseTextField.GetValue<string>();

                List<string> lines = customUseText.Split('\n').ToList();

                if (!isPlacingBasePiece && IsColliderImprovable
                    && (!__state.WereHintsEnabled
                        || !__state.WasColliderImprovable
                        || IsColliderImproved != __state.WasColliderImproved
                        || (IsSnappingEnabled && !__state.WasSnappingEnabled)))
                {
                    lines[0] += $", {ControlHint.Get((IsColliderImproved ? Localisation.OriginalCollider : Localisation.DetailedCollider).Value, Toggles.DetailedColliders)}";
                }

                lines.Insert(1, string.Join(", ",
                    ControlHint.Get(Localisation.ToggleExtendedBuildRange.Value, Toggles.ExtendBuildRange),
                    ControlHint.Get(Localisation.AdjustBuildRange.Value, Keybinds.IncreaseExtendedBuildRange.Value, Keybinds.DecreaseExtendedBuildRange.Value),
                    ControlHint.Get(Localisation.ResetBuildRange.Value, new[] { Keybinds.IncreaseExtendedBuildRange.Value, Keybinds.DecreaseExtendedBuildRange.Value })));


                if (!isPlacingBasePiece)
                {
                    lines.Insert(1, string.Join(", ",
                        ControlHint.Get(Localisation.ToggleSnapping.Value, Toggles.Snapping),
                        ControlHint.Get(Localisation.ToggleFineSnapping.Value, Toggles.FineSnapping)));
                }

                if (Builder.rotationEnabled)
                {
                    try
                    {
                        var localisedRotate = Language.main.TryGet("GhostRotateInputHint", out string str)
                            ? str.Split('(').First().Trim()
                            : "Rotate";
                        var rotateIndex = lines.FindIndex(x => x.StartsWith(localisedRotate));
                        lines[rotateIndex] += $", {ControlHint.Get(Localisation.ToggleFineRotation.Value, Toggles.FineRotation)}";
                    }
                    catch
                    {
                        lines.Add(ControlHint.Get(Localisation.ToggleFineRotation.Value, Toggles.FineRotation));
                    }
                }

                customUseText = string.Join("\n", lines.Distinct());
            }
        }

        customUseTextField.SetValue(customUseText);
        return customUseText;
    }
}
