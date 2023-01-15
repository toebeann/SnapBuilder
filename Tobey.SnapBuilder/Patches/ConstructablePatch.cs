using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Tobey.SnapBuilder.Patches;
using static Config;
internal static class ConstructablePatch
{
    public static Dictionary<Constructable, Tuple<float, float>> constructableDistances = new();

    [HarmonyPatch(typeof(Constructable), nameof(Constructable.Awake))]
    [HarmonyPostfix, HarmonyWrapSafe]
    public static void Awake_Postfix(Constructable __instance)
    {
        if (!constructableDistances.ContainsKey(__instance))
        {
            constructableDistances.Add(__instance, new(__instance.placeDefaultDistance, __instance.placeMaxDistance));
            __instance.placeDefaultDistance *= General.BuildRangeMultiplier.Value;
            __instance.placeMaxDistance *= General.BuildRangeMultiplier.Value;
        }
    }

    [HarmonyPatch(typeof(Constructable), nameof(Constructable.OnDestroy))]
    [HarmonyPrefix, HarmonyWrapSafe]
    public static void OnDestroy_Prefix(Constructable __instance)
    {
        if (constructableDistances.ContainsKey(__instance))
        {
            constructableDistances.Remove(__instance);
        }
    }
}
