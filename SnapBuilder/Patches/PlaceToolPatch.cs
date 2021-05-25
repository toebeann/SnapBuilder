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

            if (__state && SnapBuilder.Config.DisplayControlHints)
            {
                ControlHint.Show(Lang.Hint.ToggleSnapping, SnapBuilder.Config.Snapping);
                ControlHint.Show(Lang.Hint.ToggleFineSnapping, SnapBuilder.Config.FineSnapping);
            }
        }

        [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.CreateGhostModel))]
        [HarmonyPostfix]
        public static void Postfix(PlaceTool __instance, bool __state)
        {
            if (__state && SnapBuilder.Config.DisplayControlHints && __instance.rotationEnabled)
            {
                ControlHint.Show(Lang.Hint.ToggleRotation, SnapBuilder.Config.Rotation);
                ControlHint.Show(Lang.Hint.ToggleFineRotation, SnapBuilder.Config.FineRotation);
                ControlHint.Show(Lang.Hint.HolsterItem, GameInput.Button.Exit);
            }
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
                Inventory.main.quickSlots.SetIgnoreHotkeyInput(__instance.rotationEnabled && SnapBuilder.Config.Rotation.Enabled);
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
