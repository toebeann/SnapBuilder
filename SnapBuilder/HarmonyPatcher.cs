using HarmonyLib;
using QModManager.API.ModLoading;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    [QModCore]
    public static class HarmonyPatcher
    {
        [QModPatch]
        public static void ApplyPatches()
        {
            new Harmony("SnapBuilder").PatchAll();
            SnapBuilder.Initialise();
        }
    }
}
