using System.Reflection;
using Harmony;

namespace SnapBuilder
{
    internal class HarmonyPatcher
    {
        public static void ApplyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("com.tobeyblaber.subnautica.snapbuilder.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            SnapBuilder.Initialise();
        }
    }
}
