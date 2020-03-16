using Harmony;
using System.Reflection;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal class HarmonyPatcher
    {
        public static void ApplyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.tobeyblaber.straitjacket.subnautica.snapbuilder.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            SnapBuilder.Initialise();
        }
    }
}
