using System.Reflection;
using Harmony;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal class HarmonyPatcher
    {
        public static void ApplyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("SnapBuilder");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            SnapBuilder.Initialise();
        }
    }
}
