﻿using HarmonyLib;

namespace Tobey.SnapBuilder.Patches;
using static Config;
internal static class PlaceToolPatch
{
    #region PlaceTool.CreateGhostModel
    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.CreateGhostModel))]
    [HarmonyWrapSafe]
    [HarmonyPrefix]
    public static void CreateGhostModelPrefix(PlaceTool __instance, ref bool __state)
    {
        Toggles.Reset();

        __state = __instance.ghostModel == null;

        if (__state && General.DisplayControlHints.Value)
        {
            ControlHint.Show(Localisation.ToggleSnapping.Value, Toggles.Snapping);
            ControlHint.Show(Localisation.ToggleFineSnapping.Value, Toggles.FineSnapping);
        }
    }

    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.CreateGhostModel))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    public static void CreateGhostModelPostfix(PlaceTool __instance, bool __state)
    {
        if (__state && General.DisplayControlHints.Value && __instance.rotationEnabled)
        {
            ControlHint.Show(Localisation.ToggleRotation.Value, Toggles.Rotation);
            ControlHint.Show(Localisation.ToggleFineRotation.Value, Toggles.FineRotation);
            ControlHint.Show(Localisation.HolsterItem.Value, GameInput.Button.Exit);
        }
    }
    #endregion

    #region PlaceTool.LateUpdate
    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.LateUpdate))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    public static void LateUpdatePostfix(PlaceTool __instance)
    {
        if (__instance?.usingPlayer == null || !Toggles.Snapping.IsEnabled)
        {
            Inventory.main?.quickSlots.SetIgnoreHotkeyInput(false);
        }
        else
        {
            Inventory.main?.quickSlots.SetIgnoreHotkeyInput(__instance.rotationEnabled && Toggles.Rotation.IsEnabled);
        }
    }
    #endregion

    #region PlaceTool.OnPlace | PlaceTool.OnHolster
    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.OnPlace))]
    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.OnHolster))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    public static void OnPlaceOrOnHolsterPostfix() => Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
    #endregion
}
