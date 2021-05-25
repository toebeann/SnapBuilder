﻿#if BELOWZERO
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.Patches
{
    using SMLHelper;

    internal static class BuilderToolPatch
    {
        #region Builder.GetCustomUseText
        public struct GetCustomUseTextState
        {
            public readonly bool WasPlacing;
            public readonly bool WasPlacingRotatable;
            public readonly bool WasSnappingEnabled;
            public readonly bool WereHintsEnabled;
            public readonly bool WasColliderImprovable;

            public GetCustomUseTextState(bool wasPlacing, bool wasPlacingRotatable, bool wasSnappingEnabled,
                                         bool wereHintsEnabled, bool wasColliderImprovable)
            {
                WasPlacing = wasPlacing;
                WasPlacingRotatable = wasPlacingRotatable;
                WasSnappingEnabled = wasSnappingEnabled;
                WereHintsEnabled = wereHintsEnabled;
                WasColliderImprovable = wasColliderImprovable;
            }
        }

        private static bool wasSnappingEnabled = SnapBuilder.Config.Snapping.Enabled;
        private static bool wereHintsEnabled = SnapBuilder.Config.DisplayControlHints;
        private static bool wasColliderImprovable = SnapBuilder.IsColliderImprovable();
        [HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.GetCustomUseText))]
        [HarmonyPrefix]
        public static void GetCustomUseTextPrefix(BuilderTool __instance, out GetCustomUseTextState __state)
        {
            __state = new GetCustomUseTextState(wasPlacing: __instance.wasPlacing,
                                                wasPlacingRotatable: __instance.wasPlacingRotatable,
                                                wasSnappingEnabled: wasSnappingEnabled,
                                                wereHintsEnabled: wereHintsEnabled,
                                                wasColliderImprovable: wasColliderImprovable);

            wasSnappingEnabled = SnapBuilder.Config.Snapping.Enabled;
            wereHintsEnabled = SnapBuilder.Config.DisplayControlHints;
            wasColliderImprovable = SnapBuilder.IsColliderImprovable();
        }

        [HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.GetCustomUseText))]
        [HarmonyPostfix]
        public static string GetCustomUseTextPostfix(string customUseText, BuilderTool __instance, GetCustomUseTextState __state)
        {
            if (!SnapBuilder.Config.DisplayControlHints && __state.WereHintsEnabled)
            {
                __instance.UpdateCustomUseText();
                return __instance.customUseText;
            }
            else if (SnapBuilder.Config.DisplayControlHints
                     && Builder.isPlacing
                     && (!__state.WereHintsEnabled
                         || !__state.WasPlacing
                         || (SnapBuilder.Config.Snapping.Enabled != __state.WasSnappingEnabled)
                         || (SnapBuilder.IsColliderImprovable() != __state.WasColliderImprovable)
                         || (Builder.rotationEnabled != __state.WasPlacingRotatable)))
            {
                __instance.UpdateCustomUseText();
                customUseText = __instance.customUseText;

                List<string> lines = customUseText.Split('\n').ToList();

                lines[0] += $", {ControlHint.Get(Lang.Hint.ToggleSnapping, SnapBuilder.Config.Snapping)}";

                if (SnapBuilder.Config.Snapping.Enabled)
                {
                    lines.Insert(1, ControlHint.Get(Lang.Hint.ToggleFineSnapping, SnapBuilder.Config.FineSnapping));

                    if (Builder.rotationEnabled
                        && (!__state.WereHintsEnabled
                            || !__state.WasPlacingRotatable
                            || (SnapBuilder.Config.Snapping.Enabled && !__state.WasSnappingEnabled)))
                    {
                        lines[1] += $", {ControlHint.Get(Lang.Hint.ToggleFineRotation, SnapBuilder.Config.FineRotation)}";
                    }

                    if (SnapBuilder.IsColliderImprovable()
                        && (!__state.WereHintsEnabled
                            || !__state.WasColliderImprovable
                            || (SnapBuilder.Config.Snapping.Enabled && !__state.WasSnappingEnabled)))
                    {
                        lines[0] += $", {ControlHint.Get(Lang.Hint.DetailedCollider, SnapBuilder.Config.DetailedCollider)}";
                    }
                }

                customUseText = string.Join(Environment.NewLine, lines.Distinct());
            }

            return __instance.customUseText = customUseText;
        }
        #endregion
    }
}
#endif
