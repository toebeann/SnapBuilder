using HarmonyLib;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.Patch
{
    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.CreateGhostModel))]
    internal static class PlaceTool_CreateGhostModel
    {
        static void Prefix(PlaceTool __instance, ref bool __state)
        {
            SnapBuilder.Config.ResetToggles();

            __state = __instance.ghostModel == null;

            SnapBuilder.ShowSnappingHint(__state);
        }

        public static void Postfix(PlaceTool __instance, bool __state)
        {
            SnapBuilder.ShowRotationHint(__state && __instance.rotationEnabled);
        }
    }

    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.LateUpdate))]
    internal static class PlaceTool_LateUpdate
    {
        public static bool Prefix(PlaceTool __instance)
        {
            if (__instance.usingPlayer == null || !SnapBuilder.Config.Snapping.Enabled)
            {
                Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
                return true;
            }

            Inventory.main.quickSlots.SetIgnoreHotkeyInput(__instance.rotationEnabled);

            Transform aimTransform = Builder.GetAimTransform();
            RaycastHit hit;
            bool bHit = Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, 5f, PlaceTool.placeLayerMask, QueryTriggerInteraction.Ignore);
            Vector3 position = __instance.ghostModel.transform.position;
            Quaternion rotation = __instance.ghostModel.transform.rotation;

            SnapBuilder.ApplyAdditiveRotation(ref __instance.additiveRotation, out var _);

            if (bHit)
            {
                bHit = SnapBuilder.TryGetSnappedHitPoint(PlaceTool.placeLayerMask, ref hit, out Vector3 snappedHitPoint, out Vector3 snappedHitNormal);

                if (bHit)
                {
                    position = snappedHitPoint;

                    PlaceTool.SurfaceType surfaceType = PlaceTool.SurfaceType.Floor;
                    if (Mathf.Abs(hit.normal.y) < 0.3f)
                        surfaceType = PlaceTool.SurfaceType.Wall;
                    else if (hit.normal.y < 0f)
                        surfaceType = PlaceTool.SurfaceType.Ceiling;

                    if (__instance.rotationEnabled)
                    {   // New calculation of the rotation
                        rotation = SnapBuilder.CalculateRotation(ref __instance.additiveRotation, hit, snappedHitPoint, snappedHitNormal, true);
                    }
                    else
                    {   // Calculate rotation in the same manner as the original method
                        Vector3 forward;

                        if (__instance.alignWithSurface || surfaceType == PlaceTool.SurfaceType.Wall)
                            forward = hit.normal;
                        else
                            forward = new Vector3(-aimTransform.forward.x, 0f, -aimTransform.forward.z).normalized;

                        rotation = Quaternion.LookRotation(forward, Vector3.up);
                        if (__instance.rotationEnabled)
                            rotation *= Quaternion.AngleAxis(__instance.additiveRotation, Vector3.up);
                    }

                    switch (surfaceType)
                    {
                        case PlaceTool.SurfaceType.Floor:
                            __instance.validPosition = __instance.allowedOnGround;
                            break;
                        case PlaceTool.SurfaceType.Wall:
                            __instance.validPosition = __instance.allowedOnWalls;
                            break;
                        case PlaceTool.SurfaceType.Ceiling:
                            __instance.validPosition = __instance.allowedOnCeiling;
                            break;
                    }
                }
            }

            if (!bHit)
            {   // If there is no new hit, then the position we're snapping to isn't valid
                position = aimTransform.position + aimTransform.forward * 1.5f;
                rotation = Quaternion.LookRotation(-aimTransform.forward, Vector3.up);
                if (__instance.rotationEnabled)
                    rotation *= Quaternion.AngleAxis(__instance.additiveRotation, Vector3.up);
            }

            __instance.ghostModel.transform.position = position;
            __instance.ghostModel.transform.rotation = rotation;

            if (bHit)
            {
                Rigidbody componentInParent = hit.collider.gameObject.GetComponentInParent<Rigidbody>();
                __instance.validPosition = (__instance.validPosition &&
                    (componentInParent == null || componentInParent.isKinematic || __instance.allowedOnRigidBody));
            }

            SubRoot currentSub = Player.main.GetCurrentSub();

            bool isInside = Player.main.IsInsideWalkable();

            if (bHit && hit.collider.gameObject.CompareTag("DenyBuilding"))
                __instance.validPosition = false;

#if BELOWZERO
            if (!__instance.allowedUnderwater && hit.point.y < 0)
                __instance.validPosition = false;
#endif

            if (bHit && ((__instance.allowedInBase && isInside) || (__instance.allowedOutside && !isInside)))
            {
                GameObject root = UWE.Utils.GetEntityRoot(hit.collider.gameObject);
                if (!root)
                {
                    SceneObjectIdentifier identifier = hit.collider.GetComponentInParent<SceneObjectIdentifier>();
                    if (identifier)
                    {
                        root = identifier.gameObject;
                    }
                    else
                    {
                        root = hit.collider.gameObject;
                    }
                }

                if (currentSub == null)
                    __instance.validPosition &= Builder.ValidateOutdoor(root);

                if (!__instance.allowedOnConstructable)
                    __instance.validPosition &= root.GetComponentInParent<Constructable>() == null;

                __instance.validPosition &= Builder.CheckSpace(position, rotation, PlaceTool.localBounds, PlaceTool.placeLayerMask, hit.collider);
            }
            else
                __instance.validPosition = false;

            MaterialExtensions.SetColor(__instance.renderers, ShaderPropertyID._Tint,
                __instance.validPosition ? PlaceTool.placeColorAllow : PlaceTool.placeColorDeny);
            if (__instance.hideInvalidGhostModel)
            {
                __instance.ghostModel.SetActive(__instance.validPosition);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.OnPlace))]
    [HarmonyPatch(typeof(PlaceTool), nameof(PlaceTool.OnHolster))]
    internal static class PlaceTool_OnPlace_OnHolster
    {
        public static void Postfix()
        {
            Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
        }
    }
}
