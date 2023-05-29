using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Tobey.SnapBuilder.Patches;
using static Config;
internal static class BuilderPatch
{
    private static ColliderCache ColliderCache => AimTransform.Instance?.ColliderCache;

    #region Builder.Begin
    [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
    [HarmonyPrefix, HarmonyWrapSafe]
    public static void BeginHintsPrefix(GameObject modulePrefab, out bool __state)
    {
        __state =
            SnapBuilder.Instance.IsSN1 &&
            General.DisplayControlHints.Value &&
            Builder.ghostModel == null;

        if (__state && modulePrefab.GetComponent<ConstructableBase>() == null)
        {
            ControlHint.Show(Localisation.ToggleSnapping.Value, Toggles.Snapping);
            ControlHint.Show(Localisation.ToggleFineSnapping.Value, Toggles.FineSnapping);
        }
    }

    [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
    [HarmonyPostfix, HarmonyWrapSafe]
    public static void BeginHintsPostfix(GameObject modulePrefab, bool __state)
    {
        if (__state)
        {
            var isPlacingBasePiece = modulePrefab.GetComponent<ConstructableBase>() != null;

            if (!isPlacingBasePiece && Builder.rotationEnabled)
            {
                ControlHint.Show(Localisation.ToggleFineRotation.Value, Toggles.FineRotation);
            }

            if (!isPlacingBasePiece || SnapBuilder.Instance.HasLargeRoom)
            {
                ControlHint.Show(Localisation.ToggleExtendedBuildRange.Value, Toggles.ExtendBuildRange);
                ControlHint.Show(Localisation.AdjustBuildRange.Value, Keybinds.IncreaseExtendedBuildRange.Value, Keybinds.DecreaseExtendedBuildRange.Value);
                ControlHint.Show(Localisation.ResetBuildRange.Value, new[] { Keybinds.IncreaseExtendedBuildRange.Value, Keybinds.DecreaseExtendedBuildRange.Value });
            }

            if (!isPlacingBasePiece && ColliderCache?.Record?.IsImprovable == true)
            {
                string hintId = ColliderCache.Record.IsImproved
                    ? Localisation.OriginalCollider.Value
                    : Localisation.DetailedCollider.Value;
                ControlHint.Show(hintId, Toggles.DetailedColliders);
            }
        }
    }

    [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
    [HarmonyPostfix, HarmonyWrapSafe]
    public static void BeginResetTogglesPostFix() => Toggles.Reset();
    #endregion

    #region Builder.CreateGhost
    [HarmonyPatch(typeof(Builder), nameof(Builder.CreateGhost))]
    [HarmonyPostfix, HarmonyWrapSafe]
    public static void CreateGhostBuildRangePostfix()
    {
        var constructable = Builder.prefab.GetComponent<Constructable>();

        if (constructable != null)
        {
            if (!ConstructablePatch.constructableDistances.ContainsKey(constructable))
            {
                ConstructablePatch.constructableDistances.Add(constructable, new(constructable.placeDefaultDistance, constructable.placeMaxDistance));
            }

            if (Toggles.ExtendBuildRange.IsEnabled)
            {
                Builder.placeDefaultDistance = constructable.placeDefaultDistance = ConstructablePatch.constructableDistances[constructable].Item1 * ExtendedBuildRange.Multiplier.Value;
                Builder.placeMaxDistance = constructable.placeMaxDistance = ConstructablePatch.constructableDistances[constructable].Item2 * ExtendedBuildRange.Multiplier.Value;
            }
            else
            {
                Builder.placeDefaultDistance = constructable.placeDefaultDistance = ConstructablePatch.constructableDistances[constructable].Item1;
                Builder.placeMaxDistance = constructable.placeMaxDistance = ConstructablePatch.constructableDistances[constructable].Item2;
            }
        }
    }
    #endregion

    #region Builder.GetAimTransform
    [HarmonyPatch(typeof(Builder), nameof(Builder.GetAimTransform))]
    [HarmonyPostfix, HarmonyWrapSafe]
    public static Transform GetAimTransformPostfix(Transform _) => AimTransform.Instance?.GetAimTransform();
    #endregion

    #region Builder.SetPlaceOnSurface
    [HarmonyPatch(typeof(Builder), nameof(Builder.SetPlaceOnSurface))]
    [HarmonyPostfix, HarmonyWrapSafe]
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
    [HarmonyPostfix, HarmonyWrapSafe]
    public static void EndRevertCollidersPostfix() => ColliderCache.RevertAll();

    [HarmonyPatch(typeof(Builder), nameof(Builder.End))]
    [HarmonyPrefix, HarmonyWrapSafe]
    public static void EndResetConstructablesPrefix()
    {
        foreach (var key in ConstructablePatch.constructableDistances.Keys.Where(k => k == null))
        {
            ConstructablePatch.constructableDistances.Remove(key);
        }

        foreach (var entry in ConstructablePatch.constructableDistances)
        {
            Builder.placeDefaultDistance = entry.Key.placeDefaultDistance = entry.Value.Item1;
            Builder.placeMaxDistance = entry.Key.placeMaxDistance = entry.Value.Item2;
        }
    }
    #endregion
}
