using BepInEx;
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Diagnostics;
using System.Reflection;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    [QModCore]
    public static class HarmonyPatcher
    {
        private enum SupportedGame
        {
            Subnautica,
            BelowZero
        }

#if SUBNAUTICA
        private const SupportedGame TargetGame = SupportedGame.Subnautica;
#elif BELOWZERO
        private const SupportedGame TargetGame = SupportedGame.BelowZero;
#endif

        [QModPatch]
        public static void ApplyPatches()
        {
            Logger.LogInfo($"Initialising SnapBuilder for {TargetGame} v{Assembly.GetExecutingAssembly().GetName().Version}...");

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
