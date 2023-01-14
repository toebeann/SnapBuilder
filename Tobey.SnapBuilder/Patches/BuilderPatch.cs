using HarmonyLib;
using UnityEngine;

namespace Tobey.SnapBuilder.Patches;
using static Config;
internal static class BuilderPatch
{
    private static ColliderCache ColliderCache => AimTransform.Instance?.ColliderCache;

    #region Builder.Begin
    // SN1 only
    [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
    [HarmonyWrapSafe]
    [HarmonyPrefix]
    public static void BeginHintsPrefix(out bool __state)
    {
        __state = General.DisplayControlHints.Value && Builder.ghostModel == null;

        if (__state)
        {
            ControlHint.Show("Snapping", Toggles.Snapping);
            ControlHint.Show("Fine snapping", Toggles.FineSnapping);
        }
    }

    // SN1 only
    [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void BeginHintsPostfix(bool __state)
    {
        if (__state && Builder.rotationEnabled)
        {
            ControlHint.Show("Fine rotation", Toggles.FineRotation);
        }

        if (__state && (ColliderCache?.Record?.IsImprovable ?? false))
        {
            string hintId = ColliderCache.Record.IsImproved
                ? "Original collider"
                : "Detailed collider";
            ControlHint.Show(hintId, Toggles.DetailedColliders);
        }
    }

    [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void BeginResetTogglesPostFix() => Toggles.Reset();
    #endregion

    #region Builder.GetAimTransform
    [HarmonyPatch(typeof(Builder), nameof(Builder.GetAimTransform))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static Transform GetAimTransformPostfix(Transform _) => AimTransform.Instance?.GetAimTransform();
    #endregion

    #region Builder.SetPlaceOnSurface
    [HarmonyPatch(typeof(Builder), nameof(Builder.SetPlaceOnSurface))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void SetPlaceOnSurfacePostfix(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
    {
        if (Toggles.Snapping.IsEnabled)
        {
            position += SnapBuilder.Instance.GetMetadata()?.localPosition ?? Vector3.zero;

            rotation = (Builder.rotationEnabled
                ? SnapBuilder.Instance.CalculateRotation(ref Builder.additiveRotation, hit, Builder.forceUpright)
                : rotation * SnapBuilder.Instance.GetDefaultRotation());
        }
    }
    #endregion

    #region Builder.End
    [HarmonyPatch(typeof(Builder), nameof(Builder.End))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    public static void EndPostfix() => ColliderCache.RevertAll();
    #endregion
}
