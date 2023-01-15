using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tobey.SnapBuilder.Patches;
using static Config;
internal static class BuilderTool_GetCustomUseText_Patch
{
    public static MethodBase TargetMethod() => AccessTools.Method("BuilderTool:GetCustomUseText");

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

    [HarmonyWrapSafe]
    [HarmonyPrefix]
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

    [HarmonyWrapSafe]
    [HarmonyPostfix]
    public static string GetCustomUseTextPostfix(string customUseText, object __instance, GetCustomUseTextState __state)
    {
        var builderTool = Traverse.Create(__instance);
        var updateCustomUseTextMethod = builderTool.Method("UpdateCustomUseText");
        var customUseTextField = builderTool.Field("customUseText");

        if (Builder.isPlacing && Builder.prefab.GetComponent<ConstructableBase>() is null)
        {
            if (!AreHintsEnabled && __state.WereHintsEnabled)
            {
                updateCustomUseTextMethod.GetValue();
                return customUseTextField.GetValue<string>();
            }
            else if (AreHintsEnabled
                     && Builder.isPlacing
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
                lines[0] += $", {ControlHint.Get(Localisation.ToggleSnapping.Value, Toggles.Snapping)}";

                if (Toggles.Snapping.IsEnabled)
                {
                    lines.Insert(1, ControlHint.Get(Localisation.ToggleFineSnapping.Value, Toggles.FineSnapping));

                    if (Builder.rotationEnabled
                        && (!__state.WereHintsEnabled
                            || !__state.WasPlacingRotatable
                            || (IsSnappingEnabled && !__state.WasSnappingEnabled)))
                    {
                        lines[1] += $", {ControlHint.Get(Localisation.ToggleFineRotation.Value, Toggles.FineRotation)}";
                    }
                    
                    if (IsColliderImprovable
                        && (!__state.WereHintsEnabled
                            || !__state.WasColliderImprovable
                            || IsColliderImproved != __state.WasColliderImproved
                            || (IsSnappingEnabled && !__state.WasSnappingEnabled)))
                    {
                        string hint = IsColliderImproved
                            ? Localisation.OriginalCollider.Value
                            : Localisation.DetailedCollider.Value;
                        lines[0] += $", {ControlHint.Get(hint, Toggles.DetailedColliders)}";
                    }
                }

                customUseText = string.Join(Environment.NewLine, lines.Distinct());
            }
        }

        customUseTextField.SetValue(customUseText);
        return customUseText;
    }
}
