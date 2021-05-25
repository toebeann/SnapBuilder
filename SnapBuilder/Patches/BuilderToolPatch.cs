#if BELOWZERO
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

            public GetCustomUseTextState(bool wasPlacing, bool wasPlacingRotatable, bool wasSnappingEnabled, bool wereHintsEnabled)
            {
                WasPlacing = wasPlacing;
                WasPlacingRotatable = wasPlacingRotatable;
                WasSnappingEnabled = wasSnappingEnabled;
                WereHintsEnabled = wereHintsEnabled;
            }
        }

        private static bool wasSnappingEnabled = SnapBuilder.Config.Snapping.Enabled;
        private static bool wereHintsEnabled = SnapBuilder.Config.DisplayControlHints;
        [HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.GetCustomUseText))]
        [HarmonyPrefix]
        public static void GetCustomUseTextPrefix(BuilderTool __instance, out GetCustomUseTextState __state)
        {
            __state = new GetCustomUseTextState(wasPlacing: __instance.wasPlacing,
                                wasPlacingRotatable: __instance.wasPlacingRotatable,
                                wasSnappingEnabled: wasSnappingEnabled,
                                wereHintsEnabled: wereHintsEnabled);

            wasSnappingEnabled = SnapBuilder.Config.Snapping.Enabled;
            wereHintsEnabled = SnapBuilder.Config.DisplayControlHints;
        }

        [HarmonyPatch(typeof(BuilderTool), nameof(BuilderTool.GetCustomUseText))]
        [HarmonyPostfix]
        public static string GetCustomUseTextPostfix(string customUseText, BuilderTool __instance, GetCustomUseTextState __state)
        {
            if (!SnapBuilder.Config.DisplayControlHints && __state.WereHintsEnabled)
            {
                List<string> lines = customUseText.Split('\n').ToList();

                lines.RemoveAll(line => line.Contains(Language.Get(Lang.Hint.ToggleSnapping))
                                        || line.Contains(Language.Get(Lang.Hint.ToggleFineSnapping))
                                        || line.Contains(Language.Get(Lang.Hint.ToggleFineRotation)));
            }
            else if (SnapBuilder.Config.DisplayControlHints
                     && Builder.isPlacing
                     && (!__state.WereHintsEnabled
                         || !__state.WasPlacing
                         || (Builder.rotationEnabled != __state.WasPlacingRotatable)
                         || (SnapBuilder.Config.Snapping.Enabled != __state.WasSnappingEnabled)))
            {
                List<string> lines = customUseText.Split('\n').ToList();

                if (!SnapBuilder.Config.Snapping.Enabled)
                {
                    lines.RemoveAll(line => line.Contains(Language.Get(Lang.Hint.ToggleFineSnapping))
                                            || line.Contains(Language.Get(Lang.Hint.ToggleFineRotation)));

                    lines.Insert(1, ControlHint.Get(Lang.Hint.ToggleSnapping, SnapBuilder.Config.Snapping));
                }
                else
                {
                    lines.RemoveAll(line => line.Contains(Language.Get(Lang.Hint.ToggleSnapping)));

                    lines.Insert(1,
                        $"{ControlHint.Get(Lang.Hint.ToggleSnapping, SnapBuilder.Config.Snapping)}, " +
                        $"{ControlHint.Get(Lang.Hint.ToggleFineSnapping, SnapBuilder.Config.FineSnapping)}");

                    if (Builder.rotationEnabled
                        && (!__state.WereHintsEnabled
                            || !__state.WasPlacingRotatable
                            || (SnapBuilder.Config.Snapping.Enabled && !__state.WasSnappingEnabled)))
                    {
                        lines.Add(ControlHint.Get(Lang.Hint.ToggleFineRotation, SnapBuilder.Config.FineRotation));
                    }
                    else if (!Builder.rotationEnabled)
                    {
                        lines.RemoveAll(line => line.Contains(Language.Get(Lang.Hint.ToggleFineRotation)));
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
