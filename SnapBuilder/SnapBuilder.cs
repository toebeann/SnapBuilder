using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using Straitjacket.ExtensionMethods.UnityEngine;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    using BepInEx.Subnautica;
    using ExtensionMethods;
    using Patches;

    internal static class SnapBuilder
    {
        public static float LastButtonHeldTime = -1f;
        public static GameInput.Button LastButton;
        public static Config Config = OptionsPanelHandler.RegisterModOptions<Config>();

        private enum SupportedGame
        {
            Subnautica,
            BelowZero
        }

#if SUBNAUTICA
        private const SupportedGame TargetGame = SupportedGame.Subnautica;
#elif BELOWZERO
        private const SupportedGame TargetGame = SupportedGame.BelowZero;
#endif

        public static void Initialise()
        {
            Logger.LogInfo($"Initialising SnapBuilder for {TargetGame} v{Assembly.GetExecutingAssembly().GetName().Version}...");
            var stopwatch = Stopwatch.StartNew();

            ApplyHarmonyPatches();
            Config.Initialise();
            Lang.Initialise();

            stopwatch.Stop();
            Logger.LogInfo($"Initialised in {stopwatch.ElapsedMilliseconds}ms.");
        }

        private static void ApplyHarmonyPatches()
        {
            var stopwatch = Stopwatch.StartNew();

            var harmony = new Harmony("SnapBuilder");
            harmony.PatchAll(typeof(BuilderPatch));
            harmony.PatchAll(typeof(PlaceToolPatch));
#if BELOWZERO
            harmony.PatchAll(typeof(BuilderToolPatch));
#endif

            stopwatch.Stop();
            Logger.LogInfo($"Harmony patches applied in {stopwatch.ElapsedMilliseconds}ms.");
        }

        /// <summary>
        /// The camera transform, as per the original Builder.GetAimTransform()
        /// </summary>
        public static Transform BuilderAimTransform => MainCamera.camera.transform;
        public const string SnapBuilderAimTransformName = "SnapBuilderAimTransform";

        private static Transform offsetAimTransform;
        /// <summary>
        /// A non-moving parent of the MainCamera transform, to counteract head-bobbing
        /// </summary>
        public static Transform OffsetAimTransform
        {
            get
            {
                offsetAimTransform ??= BuilderAimTransform.FindAncestor("camOffset").parent
                    ?? BuilderAimTransform.FindAncestor(transform => !transform.position.Equals(OffsetAimTransform.position))
                    ?? BuilderAimTransform;
                return offsetAimTransform;
            }
        }

        private static Transform snapBuilderAimTransform;
        /// <summary>
        /// The transform attached to our custom GameObject for snapped aiming
        /// </summary>
        public static Transform SnapBuilderAimTransform
        {
            get
            {
                snapBuilderAimTransform ??= BuilderAimTransform.Find(SnapBuilderAimTransformName);
                if (snapBuilderAimTransform is null)
                {
                    snapBuilderAimTransform = new GameObject(SnapBuilderAimTransformName).transform;
                    snapBuilderAimTransform.SetParent(BuilderAimTransform, false);
                }
                return snapBuilderAimTransform;
            }
        }

        /// <summary>
        /// Where possible, use the transform of the parent as this should be a better reference point for localisation
        /// (especially useful inside a base)
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        private static Transform GetAppropriateTransform(RaycastHit hit) => Builder.GetSurfaceType(hit.normal) switch
        {
            SurfaceType.Ground => hit.transform.parent ?? hit.transform,
            _ => hit.transform
        };

        /// <summary>
        /// Get a new hit where the point and normal are localised the given transform
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static RaycastHit GetLocalisedHit(RaycastHit hit, Transform transform = null)
        {
            transform ??= hit.transform;
            hit.point = transform.InverseTransformPoint(hit.point); // Get the hit point localised relative to the hit transform
            hit.normal = transform.InverseTransformDirection(hit.normal).normalized; // Get the hit normal localised to the hit transform
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
            float roundFactor = Config.FineSnapping.Enabled ? Config.FineSnapRounding / 2f : Config.SnapRounding;

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
                        ? CalculateRotation(ref Builder.additiveRotation, hit, Builder.forceUpright || Player.main.IsInsideWalkable())
                        : Quaternion.identity;

                    // Get the corners of the object based on the Builder.bounds, localised to the hit point
                    IEnumerable<Vector3> corners = Builder.bounds
                        .Select(bounds => new { Bounds = bounds, Corners = bounds.GetCorners() })
                        .SelectMany(x => x.Corners.Select(corner => hit.point + rotation * corner));

                    // Get the farthest corner from the player
                    Vector3 farthestCorner = corners.OrderByDescending(x
                        => Vector3.Distance(x, OffsetAimTransform.position)).First();

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
                    GameObject.Destroy(child);
                    GameObject.Destroy(empty);

                    float offset
#if SUBNAUTICA
                        = 0.1f; // in subnautica, the collision boundary between objects is much larger than BZ
#elif BELOWZERO
                        = 0.0001f;
#endif

                    // Now move the hit.point outward from the wall just enough so that the object can fit
                    Vector3 poppedPoint = hit.point + hit.normal * Vector3.Distance(farthestCornerCentered, hit.point) + hit.normal * offset;

                    // Try to get a new hit by aiming at the floor from this popped point
                    if (Physics.Raycast(poppedPoint,
                                         Vector3.down,
                                         out RaycastHit poppedHit,
                                         Builder.placeMaxDistance,
                                         Builder.placeLayerMask,
                                         QueryTriggerInteraction.Ignore))
                    {
                        return poppedHit;
                    }

                    break;
            }

            return hit;
        }

        private static int lastCalculationFrame;
        public static Transform GetAimTransform()
        {
            if (!Config.Snapping.Enabled)
            {
                return BuilderAimTransform;
            }

            // Skip recalculating multiple times per frame
            if (lastCalculationFrame == Time.frameCount)
            {
                return SnapBuilderAimTransform;
            }
            lastCalculationFrame = Time.frameCount;

            // If no hit, exit early
            if (!Physics.Raycast(OffsetAimTransform.position,
                                 BuilderAimTransform.forward,
                                 out RaycastHit hit,
                                 Builder.placeMaxDistance,
                                 Builder.placeLayerMask,
                                 QueryTriggerInteraction.Ignore))
            {
                SnapBuilderAimTransform.position = OffsetAimTransform.position;
                SnapBuilderAimTransform.forward = BuilderAimTransform.forward;
                return SnapBuilderAimTransform;
            }

            Transform hitTransform = GetAppropriateTransform(hit);
            RaycastHit localisedHit = GetLocalisedHit(hit, hitTransform);
            RaycastHit snappedHit = GetSnappedHit(localisedHit);
            RaycastHit snappedWorldSpaceHit = GetWorldSpaceHit(snappedHit, hitTransform);
            RaycastHit poppedHit = PopHitOntoBestSurface(snappedWorldSpaceHit);

            SnapBuilderAimTransform.position = OffsetAimTransform.position;
            SnapBuilderAimTransform.forward = poppedHit.point - SnapBuilderAimTransform.position;

            return SnapBuilderAimTransform;
        }

        private static void ImproveHitNormal(ref RaycastHit hit)
        {
            // If the hit.collider is a MeshCollider and has a sharedMesh, it is a surface like the ground or the roof of a multipurpose room,
            // in which case we want a more accurate normal where possible
            if (hit.collider is MeshCollider meshCollider && meshCollider.sharedMesh is Mesh)
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
                    Physics.Raycast(hit.point + Vector3.up * .1f + offset * .2f,
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
                    if (!normal.Equals(hit.normal))
                    {   // If the normal is not the same as the original, add it to the normal and average
                        hit.normal += normal;
                        hit.normal /= 2;
                    }
                }

                // For sanity's sake, make sure the normal is normalised after summing & averaging
                hit.normal = hit.normal.normalized;
            }
        }

        private static void ApplyAdditiveRotation(ref float additiveRotation)
        {
            // If the user is rotating, apply the additive rotation
            if (GameInput.GetButtonHeld(Builder.buttonRotateCW)) // Clockwise
            {
                if (LastButton != Builder.buttonRotateCW)
                {   // Clear previous rotation held time
                    LastButton = Builder.buttonRotateCW;
                    LastButtonHeldTime = -1f;
                }

                float buttonHeldTime = Math.FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCW), 0.15f);
                if (buttonHeldTime > LastButtonHeldTime)
                {   // Store rotation held time
                    LastButtonHeldTime = buttonHeldTime;
                    additiveRotation -= Config.RotationFactor; // Decrement rotation
                }
            }
            else if (GameInput.GetButtonHeld(Builder.buttonRotateCCW)) // Counter-clockwise
            {
                if (LastButton != Builder.buttonRotateCCW)
                {   // Clear previous rotation held time
                    LastButton = Builder.buttonRotateCCW;
                    LastButtonHeldTime = -1f;
                }

                float buttonHeldTime = Math.FloorToNearest(GameInput.GetButtonHeldTime(Builder.buttonRotateCCW), 0.15f);
                if (buttonHeldTime > LastButtonHeldTime)
                {   // Store rotation held time
                    LastButtonHeldTime = buttonHeldTime;
                    additiveRotation += Config.RotationFactor; // Increment rotation
                }
            }
            else if (GameInput.GetButtonUp(Builder.buttonRotateCW) || GameInput.GetButtonUp(Builder.buttonRotateCCW))
            {   // User is not rotating, clear rotation held time
                LastButtonHeldTime = -1f;
            }

            // Round to the nearest rotation factor for rotation snapping
            additiveRotation %= 360;
        }

        public static Quaternion CalculateRotation(ref float additiveRotation, RaycastHit hit, bool forceUpright)
        {
            ApplyAdditiveRotation(ref additiveRotation);
            ImproveHitNormal(ref hit);

            Transform hitTransform = GetAppropriateTransform(hit);

            // Instantiate empty game objects for applying rotations
            GameObject empty = new GameObject();
            GameObject child = new GameObject();
            empty.transform.position = hit.point; // Set the parent transform's position to our chosen position

            // choose whether we should use the global forward, or the forward of the hitTransform
            Vector3 forward = hitTransform.forward.y != 0
                && !Player.main.IsInsideWalkable()
                && hitTransform.GetComponent<BaseCell>() is null
                    ? Vector3.forward
                    : hitTransform.forward;

            // align the empty to face the chosen forward direction
            empty.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            // for components that are not forced upright, align the empty's up direction with the hit.normal
            if (!forceUpright)
            {
                empty.transform.up = hit.normal;
                empty.transform.rotation *= Quaternion.FromToRotation(Vector3.forward, forward);
            }

            child.transform.SetParent(empty.transform, false); // parent the child to the empty

            // Rotate the child transform to look at the player (so that the object will face the player by default, as in the original)
            child.transform.LookAt(Player.main.transform);

            // Round/snap the Y axis of the child transform's local rotation based on the user's rotation factor, after adding the additiveRotation
            child.transform.localEulerAngles
                = new Vector3(0, Math.RoundToNearest(child.transform.localEulerAngles.y + additiveRotation, Config.RotationFactor) % 360, 0);

            Quaternion rotation = child.transform.rotation; // Our final rotation

            // Clean up after ourselves
            GameObject.DestroyImmediate(child);
            GameObject.DestroyImmediate(empty);

            return rotation;
        }
    }
}
