using BepInEx;
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Diagnostics;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    [QModCore]
    public static class HarmonyPatcher
    {
        [QModPatch]
        public static void ApplyPatches()
        {
            Logger.LogInfo("Initialising...");

            var stopwatch = Stopwatch.StartNew();
            new Harmony("SnapBuilder").PatchAll();
            stopwatch.Stop();
            Logger.LogInfo($"Harmony patches applied in {stopwatch.ElapsedMilliseconds}ms.");

            stopwatch.Restart();
            SnapBuilder.Initialise();
            stopwatch.Stop();
            Logger.LogInfo($"Initialised in {stopwatch.ElapsedMilliseconds}ms.");
        }
    }
}
