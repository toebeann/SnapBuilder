using HarmonyLib;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.Patches
{
    internal static class BuilderPatch
    {
        #region Builder.Begin
#if SUBNAUTICA
        [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
        [HarmonyPrefix]
        public static void BeginHintsPrefix(out bool __state)
        {
            __state = SnapBuilder.Config.DisplayControlHints && Builder.ghostModel == null;

            if (__state)
            {
                ControlHint.Show(Lang.Hint.ToggleSnapping, SnapBuilder.Config.Snapping);
                ControlHint.Show(Lang.Hint.ToggleFineSnapping, SnapBuilder.Config.FineSnapping);
            }
        }

        [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
        [HarmonyPostfix]
        public static void BeginHintsPostfix(bool __state)
        {
            if (__state && Builder.rotationEnabled)
            {
                ControlHint.Show(Lang.Hint.ToggleFineRotation, SnapBuilder.Config.FineRotation);
            }
            if (__state && (ColliderCache.Main.Record?.IsImprovable ?? false))
            {
                string hintId = ColliderCache.Main.Record.IsImproved ? Lang.Hint.OriginalCollider : Lang.Hint.DetailedCollider;
                ControlHint.Show(hintId, SnapBuilder.Config.DetailedCollider);
            }
        }
#endif

        [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
        [HarmonyPostfix]
        public static void BeginResetTogglesPostfix() => SnapBuilder.Config.ResetToggles();
        #endregion

        #region Builder.GetAimTransform
        [HarmonyPatch(typeof(Builder), nameof(Builder.GetAimTransform))]
        [HarmonyPostfix]
        public static Transform GetAimTransformPostfix(Transform _) => AimTransform.Main.GetAimTransform();
        #endregion

        #region Builder.SetPlaceOnSurface
        [HarmonyPatch(typeof(Builder), nameof(Builder.SetPlaceOnSurface))]
        [HarmonyPostfix]
        public static void SetPlaceOnSurfacePostfix(RaycastHit hit, ref Quaternion rotation)
        {
            if (SnapBuilder.Config.Snapping.Enabled && Builder.rotationEnabled)
            {
                rotation = SnapBuilder.CalculateRotation(ref Builder.additiveRotation, hit, Builder.forceUpright);
            }
        }
        #endregion

        #region Builder.End
        [HarmonyPatch(typeof(Builder), nameof(Builder.End))]
        [HarmonyPostfix]
        public static void EndPostfix()
        {
            ColliderCache.Main.RevertAll();
        }
        #endregion
    }
}
