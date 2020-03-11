using Harmony;

namespace SnapBuilder
{
    [HarmonyPatch(typeof(Builder))]
    [HarmonyPatch("Begin")]
    static class Builder_Begin_Patch
    {
        static void Prefix()
        {
            SnapBuilder.Options.Snapping.Reset();
            SnapBuilder.Options.FineSnapping.Reset();
            SnapBuilder.Options.FineRotation.Reset();
        }
    }
}
