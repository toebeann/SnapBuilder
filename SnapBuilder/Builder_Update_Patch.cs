using UnityEngine;
using Harmony;

namespace SnapBuilder
{
    [HarmonyPatch(typeof(Builder))]
    [HarmonyPatch("Update")]
    static class Builder_Update_Patch
    {
        static void Postfix()
        {
            if (Input.GetKeyUp(SnapBuilder.Options.ToggleSnappingKey))
            {
                SnapBuilder.Enabled = !SnapBuilder.Enabled;
            }
        }
    }
}
