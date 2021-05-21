using HarmonyLib;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.Patches
{
    internal static class PlaceToolPatch
    {
        #region PlaceTool.CreateGhostModel
        [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.CreateGhostModel))]
        [HarmonyPrefix]
        public static void CreateGhostModelPrefix(PlaceTool __instance, ref bool __state)
        {
            SnapBuilder.Config.ResetToggles();

            __state = __instance.ghostModel == null;

            SnapBuilder.ShowSnappingHint(__state);
        }

        [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.CreateGhostModel))]
        [HarmonyPostfix]
        public static void Postfix(PlaceTool __instance, bool __state)
        {
            SnapBuilder.ShowToggleRotationHint(__state && __instance.rotationEnabled);
            SnapBuilder.ShowToggleFineRotationHint(__state && __instance.rotationEnabled);
            SnapBuilder.ShowHolsterHint(__state && __instance.rotationEnabled);
        }
        #endregion

        #region PlaceTool.LateUpdate
        [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.LateUpdate))]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(PlaceTool __instance)
        {
            if (__instance.usingPlayer == null || !SnapBuilder.Config.Snapping.Enabled)
            {
                Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
            }
            else
            {
                Inventory.main.quickSlots.SetIgnoreHotkeyInput(__instance.rotationEnabled && SnapBuilder.Config.ToggleRotation.Enabled);
            }
        }
        #endregion

        #region PlaceTool.OnPlace & PlaceTool.OnHolster
        [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.OnPlace))]
        [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.OnHolster))]
        [HarmonyPostfix]
        public static void OnPlaceOrOnHolsterPostfix()
        {
            Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
        }
        #endregion
    }
}
