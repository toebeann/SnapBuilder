using Straitjacket.ExtensionMethods.UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    using ExtensionMethods;

    internal class AimTransform : MonoBehaviour
    {
        private static AimTransform main;
        public static AimTransform Main => main == null
            ? new GameObject("SnapBuilder").AddComponent<AimTransform>()
            : main;

        public static bool Raycast(Vector3 from, Vector3 direction, out RaycastHit hit) =>
            Physics.Raycast(from, direction, out hit, Builder.placeMaxDistance,
                Builder.placeLayerMask, QueryTriggerInteraction.Ignore);

        /// <summary>
        /// The camera transform, as per the original Builder.GetAimTransform()
        /// </summary>
        public Transform BuilderAimTransform => MainCamera.camera.transform;

        private Transform offsetAimTransform;
        /// <summary>
        /// A non-moving parent of the MainCamera transform, to counteract head-bobbing
        /// </summary>
        public Transform OffsetAimTransform => offsetAimTransform ??=
            BuilderAimTransform.FindAncestor("camOffset").parent
                ?? BuilderAimTransform.FindAncestor(transform => !transform.position.Equals(BuilderAimTransform.position))
                ?? BuilderAimTransform;

        private int lastCalculationFrame;

        private Transform GetOrientedTransform(Vector3? position = null, Vector3? forward = null)
        {
            position ??= OffsetAimTransform.position;
            transform.position = position.Value;

            forward ??= BuilderAimTransform.forward;
            transform.forward = forward.Value;

            return transform;
        }

        /// <summary>
        /// A replacement for <see cref="Builder.GetAimTransform"/> that performs all 
        /// appropriate snapping calculations ahead of returning the modified transform.
        /// </summary>
        /// <returns></returns>
        public Transform GetAimTransform()
        {
            if (!SnapBuilder.Config.Snapping.Enabled)
            {
                return BuilderAimTransform;
            }

            // Skip recalculating multiple times per frame
            if (lastCalculationFrame == Time.frameCount)
            {
                return transform;
            }
            lastCalculationFrame = Time.frameCount;

            // If no hit, exit early
            if (!Raycast(OffsetAimTransform.position,
                         BuilderAimTransform.forward,
                         out RaycastHit hit))
            {
                return GetOrientedTransform();
            }

            RaycastHit improvedColliderHit = GetImprovedColliderHit(hit);
            if (improvedColliderHit.collider is null)
            {
                return GetOrientedTransform();
            }

            Transform hitTransform = improvedColliderHit.GetOptimalTransform();
            RaycastHit localisedHit = GetLocalisedHit(improvedColliderHit, hitTransform);
            RaycastHit snappedHit = GetSnappedHit(localisedHit);
            RaycastHit worldSpaceHit = GetWorldSpaceHit(snappedHit, hitTransform);
            RaycastHit poppedHit = PopHitOntoBestSurface(worldSpaceHit);

            return GetOrientedTransform(forward: poppedHit.point - transform.position);
        }

        /// <summary>
        /// Where applicable, gets a new hit after improving/reverting the collider
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        private RaycastHit GetImprovedColliderHit(RaycastHit hit)
        {
            if (ColliderCache.Main.GetRecord(hit.collider) is ColliderRecord record && record.IsImprovable)
            {
                if (SnapBuilder.Config.DetailedCollider.Enabled)
                {
                    record.Improve();
                    if (record.IsImproved)
                    {
                        Raycast(OffsetAimTransform.position, BuilderAimTransform.forward, out hit);
                    }
                }
                else if (!SnapBuilder.Config.DetailedCollider.Enabled)
                {
                    record.Revert();
                    if (!record.IsImproved)
                    {
                        Raycast(OffsetAimTransform.position, BuilderAimTransform.forward, out hit);
                    }
                }

                if (hit.collider is Collider
                    && SnapBuilder.Config.RenderImprovableColliders)
                {
                    record.Render();
                }
            }

            return hit;
        }

        /// <summary>
        /// Get a new hit where the point and normal are localised the given transform
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private RaycastHit GetLocalisedHit(RaycastHit hit, Transform transform = null)
        {
            transform ??= hit.transform;
            hit.point = transform.InverseTransformPoint(hit.point); // Get the hit point localised relative to the hit transform
            hit.normal = transform.InverseTransformDirection(hit.normal).normalized; // Get the hit normal localised to the hit transform
            return hit;
        }

        /// <summary>
        /// Gets a new hit where the point is snapped based on the normal and current round factor
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        /// <returns></returns>
        private static RaycastHit GetSnappedHit(RaycastHit hit)
        {
            Vector3 hitPoint = hit.point;
            Vector3 hitNormal = hit.normal;

            hitNormal.x = Mathf.Abs(hitNormal.x);
            hitNormal.y = Mathf.Abs(hitNormal.y);
            hitNormal.z = Mathf.Abs(hitNormal.z);
            hitNormal = hitNormal.normalized; // For sanity's sake, make sure the normal is normalised

            // Get the rounding factor from user options based on whether the fine snapping key is held or not
            float roundFactor = SnapBuilder.Config.FineSnapping.Enabled ? SnapBuilder.Config.FineSnapRounding / 2f : SnapBuilder.Config.SnapRounding;

            // Round (snap) the localised hit point coords only on axes where the corresponding normal axis is less than 1
            if (hitNormal.x < 1)
            {
                hitPoint.x = Math.RoundToNearest(hitPoint.x, roundFactor);
            }
            if (hitNormal.y < 1)
            {
                hitPoint.y = Math.RoundToNearest(hitPoint.y, roundFactor);
            }
            if (hitNormal.z < 1)
            {
                hitPoint.z = Math.RoundToNearest(hitPoint.z, roundFactor);
            }

            hit.point = hitPoint;
            return hit;
        }

        /// <summary>
        /// Gets a new hit in world space
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static RaycastHit GetWorldSpaceHit(RaycastHit hit, Transform transform = null)
        {
            transform ??= hit.transform;
            hit.point = transform.TransformPoint(hit.point);
            hit.normal = transform.TransformDirection(hit.normal).normalized;
            return hit;
        }

        /// <summary>
        /// Gets a new hit popped onto the most appropriate surface at the most appropriate point,
        /// or the original hit if the operation is not possible
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        private static RaycastHit PopHitOntoBestSurface(RaycastHit hit)
        {
            if (!Player.main.IsInsideWalkable())
                return hit;

            switch (Builder.GetSurfaceType(hit.normal))
            {
                case SurfaceType.Wall
                when !Builder.allowedSurfaceTypes.Contains(SurfaceType.Wall)
                     && Builder.allowedSurfaceTypes.Contains(SurfaceType.Ground):

                    // Get the rotation of the object
                    Quaternion rotation = Builder.rotationEnabled
                        ? SnapBuilder.CalculateRotation(ref Builder.additiveRotation, hit, Builder.forceUpright || Player.main.IsInsideWalkable())
                        : Quaternion.identity;

                    if (!Builder.bounds.Any())
                    {
                        return hit; // if there are no bounds for some reason, just use the original hit
                    }

                    // Get the corners of the object based on the Builder.bounds, localised to the hit point
                    IEnumerable<Vector3> corners = Builder.bounds
                        .Select(bounds => new { Bounds = bounds, Corners = bounds.GetCorners() })
                        .SelectMany(x => x.Corners.Select(corner => hit.point + rotation * corner));

                    // Get the farthest corner from the player
                    Vector3 farthestCorner = corners.OrderByDescending(x
                        => Vector3.Distance(x, AimTransform.Main.OffsetAimTransform.position)).First();

                    // Center the corner to the hit.point on the local X and Y axes
                    var empty = new GameObject();
                    var child = new GameObject();
                    empty.transform.position = hit.point;
                    empty.transform.forward = hit.normal;
                    child.transform.SetParent(empty.transform);
                    child.transform.position = farthestCorner;
                    child.transform.localPosition = new Vector3(0, 0, child.transform.localPosition.z);
                    Vector3 farthestCornerCentered = child.transform.position;

                    // Clean up the GameObjects as we don't need them anymore
                    Destroy(child);
                    Destroy(empty);

                    float offset
#if SUBNAUTICA
                        = 0.1f; // in subnautica, the collision boundary between objects is much larger than BZ
#elif BELOWZERO
                        = 0.02f;
#endif

                    // Now move the hit.point outward from the wall just enough so that the object can fit
                    Vector3 poppedPoint = hit.point + hit.normal * Vector3.Distance(farthestCornerCentered, hit.point) + hit.normal * offset;

                    // Try to get a new hit by aiming at the floor from this popped point
                    if (Raycast(poppedPoint, Vector3.down, out RaycastHit poppedHit))
                    {
                        return poppedHit;
                    }

                    break;
            }

            return hit;
        }

        private void Awake()
        {
            if (main != null && main != this)
            {
                Destroy(this);
            }
            else
            {
                main = this;
                transform.SetParent(BuilderAimTransform, false);
            }
        }
    }
}
