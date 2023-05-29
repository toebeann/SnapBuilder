using HarmonyLib;
using System;
using UnityEngine;

namespace Tobey.SnapBuilder.Patches;
using static Config;
internal static class PhysicsPatch
{
    [HarmonyPatch(typeof(UWE.Utils), nameof(UWE.Utils.RaycastIntoSharedBuffer))]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3), typeof(float), typeof(int), typeof(QueryTriggerInteraction) })]
    [HarmonyPrefix, HarmonyWrapSafe]
    public static void RaycastIntoSharedBufferPrefix(Vector3 origin, Vector3 direction, ref float maxDistance)
    {
        if (AimTransform.GetPlaceTool() != null
            && Vector3.Distance(origin, AimTransform.Instance.transform.position) <= Vector3.kEpsilon
            && Vector3.Distance(direction, AimTransform.Instance.transform.forward) <= Vector3.kEpsilon)
        {
            maxDistance *= ExtendedBuildRange.Multiplier.Value;
        }
    }

    [HarmonyPatch(typeof(Targeting), nameof(Targeting.GetTarget))]
    [HarmonyPatch(
        argumentTypes: new Type[] { typeof(float), typeof(GameObject), typeof(float) },
        argumentVariations: new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
    [HarmonyPrefix, HarmonyWrapSafe]
    public static void GetTargetPrefix(ref float maxDistance)
    {
        if (AimTransform.GetBuilderTool() != null && Builder.isPlacing && Toggles.ExtendBuildRange.IsEnabled)
        {
            maxDistance *= ExtendedBuildRange.Multiplier.Value;
        }
    }
}
