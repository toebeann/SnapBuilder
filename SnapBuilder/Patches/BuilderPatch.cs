using HarmonyLib;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.Patches
{
    internal static class BuilderPatch
    {
        #region Builder.Begin
        [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
        [HarmonyPrefix]
        public static void BeginPrefix(ref bool __state)
        {
            SnapBuilder.Config.ResetToggles();

            __state = Builder.ghostModel == null;

            if (__state)
            {
                ControlHint.Show(Lang.Hint.ToggleSnapping, SnapBuilder.Config.Snapping);
                ControlHint.Show(Lang.Hint.ToggleFineSnapping, SnapBuilder.Config.FineSnapping);
            }
        }

        [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
        [HarmonyPostfix]
        public static void BeginPostfix(bool __state)
        {
            if (__state && Builder.rotationEnabled)
            {
                ControlHint.Show(Lang.Hint.ToggleFineRotation, SnapBuilder.Config.FineRotation);
            }
        }
        #endregion

        #region Builder.GetAimTransform
        [HarmonyPatch(typeof(Builder), nameof(Builder.GetAimTransform))]
        [HarmonyPostfix]
        public static Transform GetAimTransformPostfix(Transform _) => SnapBuilder.GetAimTransform();
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
    }
}
