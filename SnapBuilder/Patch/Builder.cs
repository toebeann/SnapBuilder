using HarmonyLib;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.Patch
{
    [HarmonyPatch(typeof(Builder), nameof(Builder.Begin))]
    internal static class Builder_Begin
    {
        static void Prefix(ref bool __state)
        {
            SnapBuilder.Config.ResetToggles();

            __state = Builder.ghostModel == null;

            SnapBuilder.ShowSnappingHint(__state);
        }

        public static void Postfix(bool __state)
        {
            SnapBuilder.ShowRotationHint(__state && Builder.rotationEnabled);
        }
    }

    [HarmonyPatch(typeof(Builder), nameof(Builder.SetPlaceOnSurface))]
    internal static class Builder_SetPlaceOnSurface
    {
        public static bool Prefix(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
        {
            if (!SnapBuilder.Config.Snapping.Enabled)
            {
                return true; // Pass to the original function if SnapBuilder is disabled
            }

            Transform aimTransform = Builder.GetAimTransform();

            if (!SnapBuilder.TryGetSnappedHitPoint(ref hit, out Vector3 snappedHitPoint, out Vector3 snappedHitNormal, aimTransform))
            {   // If there is no new hit, then the position we're snapping to isn't valid and we can just return false
                // without setting the position or rotation and it will be treated as if no hit occurred
                return false;
            }

            // Set the position equal to the new hit point
            position = snappedHitPoint;

            if (Builder.rotationEnabled)
            {   // New calculation of the rotation
                rotation = SnapBuilder.CalculateRotation(ref Builder.additiveRotation, hit, snappedHitPoint, snappedHitNormal);
            }
            else
            {   // Calculate rotation in the same manner as the original method
                Vector3 vector = Vector3.forward;
                Vector3 vector2 = Vector3.up;

                if (Builder.forceUpright)
                {
                    vector = -aimTransform.forward;
                    vector.y = 0f;
                    vector.Normalize();
                    vector2 = Vector3.up;
                }
                else
                {
                    switch (Builder.GetSurfaceType(snappedHitNormal))
                    {
                        case SurfaceType.Ground:
                            vector2 = snappedHitNormal;
                            vector = -aimTransform.forward;
                            vector.y -= Vector3.Dot(vector, vector2);
                            vector.Normalize();
                            break;
                        case SurfaceType.Wall:
                            vector = snappedHitNormal;
                            vector2 = Vector3.up;
                            break;
                        case SurfaceType.Ceiling:
                            vector = snappedHitNormal;
                            vector2 = -aimTransform.forward;
                            vector2.y -= Vector3.Dot(vector2, vector);
                            vector2.Normalize();
                            break;
                    }
                }

                rotation = Quaternion.LookRotation(vector, vector2);
            }

            return false; // Do not run the original method
        }
    }
}
