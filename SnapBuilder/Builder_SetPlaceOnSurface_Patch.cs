using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace SnapBuilder
{
    [HarmonyPatch(typeof(Builder))]
    [HarmonyPatch("SetPlaceOnSurface")]
    static class Builder_SetPlaceOnSurface_Patch
    {
        static bool Prefix(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
        {
            if (!SnapBuilder.Options.Snapping.Enabled)
            {
                return true; // Pass to the original function if SnapBuilder is disabled
            }

            Vector3 localPoint = hit.transform.InverseTransformPoint(hit.point); // Get the hit point localised relative to the hit transform
            Vector3 localNormal = hit.transform.InverseTransformDirection(hit.normal).normalized; // Get the hit normal localised to the hit transform

            // Set the localised normal to absolute values for comparison
            localNormal.x = Mathf.Abs(localNormal.x);
            localNormal.y = Mathf.Abs(localNormal.y);
            localNormal.z = Mathf.Abs(localNormal.z);
            localNormal = localNormal.normalized; // For sanity's sake, make sure the normal is normalised

            // Get the rounding factor from user options based on whether the fine snapping key is held or not
            float roundFactor = SnapBuilder.Options.FineSnapping.Enabled ? SnapBuilder.Options.FineSnapRounding : SnapBuilder.Options.SnapRounding;

            // Round (snap) the localised hit point coords only on axes where the corresponding normal axis is less than 1
            if (localNormal.x < 1)
            {
                localPoint.x = SnapBuilder.RoundToNearest(localPoint.x, roundFactor);
            }
            if (localNormal.y < 1)
            {
                localPoint.y = SnapBuilder.RoundToNearest(localPoint.y, roundFactor);
            }
            if (localNormal.z < 1)
            {
                localPoint.z = SnapBuilder.RoundToNearest(localPoint.z, roundFactor);
            }

            // Now, perform a new raycast so that we can get the normal of the new position
            Transform aimTransform = Builder.GetAimTransform();
            Physics.Raycast(aimTransform.position,
                hit.transform.TransformPoint(localPoint) - aimTransform.position, // direction from the aim transform to the new world space position of the rounded/snapped position
                out hit, // overwrite hit
                Builder.placeMaxDistance,
                Builder.placeLayerMask.value,
                QueryTriggerInteraction.Ignore);

            // Set the position equal to the new hit point
            position = hit.point;

            Vector3 hitNormal = hit.normal; // Store the hit.normal as we may need to change this in certain circumstances

            // If the hit.collider is a MeshCollider and has a sharedMesh, it is a surface like the ground or the roof of a multipurpose room,
            // in which case we want a more accurate normal where possible
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null)
            {
                // Set up the offsets for raycasts around the point
                Vector3[] offsets = new Vector3[]
                {
                    Vector3.forward,
                    Vector3.back,
                    Vector3.left,
                    Vector3.right,
                    (Vector3.forward + Vector3.right) * .707f,
                    (Vector3.forward + Vector3.left) * .707f,
                    (Vector3.back + Vector3.right) * .707f,
                    (Vector3.back + Vector3.left) * .707f
                };

                List<Vector3> normals = new List<Vector3>();
                // Perform a raycast from each offset, pointing down toward the surface
                foreach (Vector3 offset in offsets)
                {
                    Physics.Raycast(position + Vector3.up * .1f + offset * .2f,
                        Vector3.down,
                        out RaycastHit offsetHit,
                        Builder.placeMaxDistance,
                        Builder.placeLayerMask.value,
                        QueryTriggerInteraction.Ignore);
                    if (offsetHit.transform == hit.transform) // If we hit the same object, add the hit normal
                    {
                        normals.Add(offsetHit.normal);
                    }
                }

                foreach (Vector3 normal in normals)
                {
                    if (normal != hit.normal)
                    {   // If the normal is not the same as the original, add it to the normal and average
                        hitNormal += normal / 2;
                    }
                }

                // For sanity's sake, make sure the normal is normalised after summing & averaging
                hitNormal = hitNormal.normalized;
            }

            if (Builder.rotationEnabled)
            {   // New calculation of the rotation

                // Get the rotation factor from user options based on whether the fine snapping key is held or not
                float rotationFactor = SnapBuilder.Options.FineRotation.Enabled ? SnapBuilder.Options.FineRotationRounding : SnapBuilder.Options.RotationRounding;

                // If the user is rotating, apply the additive rotation
                if (GameInput.GetButtonHeld(Builder.buttonRotateCW)) // Clockwise
                {
                    if (SnapBuilder.LastButton != Builder.buttonRotateCW)
                    {   // Clear previous rotation held time
                        SnapBuilder.LastButton = Builder.buttonRotateCW;
                        SnapBuilder.LastButtonHeldTime = -1f;
                    }

                    float buttonHeldTime = SnapBuilder.FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCW), 0.15f);
                    if (buttonHeldTime > SnapBuilder.LastButtonHeldTime)
                    {   // Store rotation held time
                        SnapBuilder.LastButtonHeldTime = buttonHeldTime;
                        Builder.additiveRotation -= rotationFactor; // Decrement rotation
                    }
                }
                else if (GameInput.GetButtonHeld(Builder.buttonRotateCCW)) // Counter-clockwise
                {
                    if (SnapBuilder.LastButton != Builder.buttonRotateCCW)
                    {   // Clear previous rotation held time
                        SnapBuilder.LastButton = Builder.buttonRotateCCW;
                        SnapBuilder.LastButtonHeldTime = -1f;
                    }

                    float buttonHeldTime = SnapBuilder.FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCCW), 0.15f);
                    if (buttonHeldTime > SnapBuilder.LastButtonHeldTime)
                    {   // Store rotation held time
                        SnapBuilder.LastButtonHeldTime = buttonHeldTime;
                        Builder.additiveRotation += rotationFactor; // Increment rotation
                    }
                }
                else if (GameInput.GetButtonUp(Builder.buttonRotateCW) || GameInput.GetButtonUp(Builder.buttonRotateCCW))
                {   // User is not rotating, clear rotation held time
                    SnapBuilder.LastButtonHeldTime = -1f;
                }

                // Round to the nearest rotation factor for rotation snapping
                Builder.additiveRotation = SnapBuilder.RoundToNearest(Builder.additiveRotation, rotationFactor) % 360;

                Transform hitTransform = hit.transform;
                if (!Player.main.IsInSub())
                {   // If the player is outside, get the root transform if there is one, otherwise default to the original
                    hitTransform = UWE.Utils.GetEntityRoot(hit.transform.gameObject)?.transform ?? hit.transform;
                }

                // Instantiate empty game objects for applying rotations
                GameObject empty = new GameObject();
                GameObject child = new GameObject();
                child.transform.parent = empty.transform; // parent the child to the empty
                child.transform.localPosition = Vector3.zero; // Make sure the child's local position is Vector3.zero
                empty.transform.position = position; // Set the parent transform's position to our chosen position

                empty.transform.forward = hitTransform.forward; // Set the parent transform's forward to match the forward of the hit.transform
                if (!Builder.forceUpright)
                {   // Rotate the parent transform so that it's Y axis is aligned with the hit.normal, but only when if it isn't forced upright
                    empty.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * empty.transform.rotation;
                }

                child.transform.LookAt(Player.main.transform); // Rotate the child transform to look at the player (so that the object will face the player by default, as in the original)
                child.transform.localEulerAngles
                    = new Vector3(0,
                    SnapBuilder.RoundToNearest(child.transform.localEulerAngles.y + Builder.additiveRotation, rotationFactor) % 360,
                    0); // Round/snap the Y axis of the child transform's local rotation based on the user's rotation factor, after adding the additiveRotation

                rotation = child.transform.rotation; // Our final rotation

                // Clean up after ourselves
                GameObject.DestroyImmediate(child);
                GameObject.DestroyImmediate(empty);
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
                    switch (Builder.GetSurfaceType(hitNormal))
                    {
                        case SurfaceType.Ground:
                            vector2 = hitNormal;
                            vector = -aimTransform.forward;
                            vector.y -= Vector3.Dot(vector, vector2);
                            vector.Normalize();
                            break;
                        case SurfaceType.Wall:
                            vector = hitNormal;
                            vector2 = Vector3.up;
                            break;
                        case SurfaceType.Ceiling:
                            vector = hitNormal;
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
