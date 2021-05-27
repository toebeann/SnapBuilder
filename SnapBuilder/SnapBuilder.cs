using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SMLHelper.V2.Handlers;
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
        public static Cache Cache = new Cache();

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
                        => Vector3.Distance(x, Cache.OffsetAimTransform.position)).First();

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

        private static Color ImprovedCollider { get; } = Color.black;
        private static Color OriginalCollider { get; } = Color.gray;

        private static int lastCalculationFrame;
        public static Transform GetAimTransform()
        {
            if (!Config.Snapping.Enabled)
            {
                return Cache.BuilderAimTransform;
            }

            // Skip recalculating multiple times per frame
            if (lastCalculationFrame == Time.frameCount)
            {
                return Cache.SnapBuilderAimTransform;
            }
            lastCalculationFrame = Time.frameCount;

            // If no hit, exit early
            if (!Physics.Raycast(Cache.OffsetAimTransform.position,
                                 Cache.BuilderAimTransform.forward,
                                 out RaycastHit hit,
                                 Builder.placeMaxDistance,
                                 Builder.placeLayerMask,
                                 QueryTriggerInteraction.Ignore))
            {
                Cache.LastCollider = null;

                Cache.SnapBuilderAimTransform.position = Cache.OffsetAimTransform.position;
                Cache.SnapBuilderAimTransform.forward = Cache.BuilderAimTransform.forward;
                return Cache.SnapBuilderAimTransform;
            }

            Cache.LastCollider = hit.collider;
            if (IsColliderImprovable())
            {
                if (Config.DetailedCollider.Enabled && !IsColliderImproved())
                {
                    ImproveColliderAndUpdateHit(ref hit);

                    if (hit.collider is null)
                    {
                        Cache.LastCollider = null;

                        Cache.SnapBuilderAimTransform.position = Cache.OffsetAimTransform.position;
                        Cache.SnapBuilderAimTransform.forward = Cache.BuilderAimTransform.forward;
                        return Cache.SnapBuilderAimTransform;
                    }
                }
                else if (!Config.DetailedCollider.Enabled && IsColliderImproved())
                {
                    RevertColliderAndUpdateHit(ref hit);

                    if (hit.collider is null)
                    {
                        Cache.LastCollider = null;

                        Cache.SnapBuilderAimTransform.position = Cache.OffsetAimTransform.position;
                        Cache.SnapBuilderAimTransform.forward = Cache.BuilderAimTransform.forward;
                        return Cache.SnapBuilderAimTransform;
                    }
                }

                Cache.LastCollider = hit.collider;
                Cache.ColliderMaterial.SetColor(ShaderPropertyID._Tint, IsColliderImproved() ? ImprovedCollider : OriginalCollider);

                if (Config.RenderImprovableColliders)
                {
                    RenderCollider(hit.collider, Cache.ColliderMaterial, 1.00001f);
                }
            }

            Transform hitTransform = GetAppropriateTransform(hit);
            RaycastHit localisedHit = GetLocalisedHit(hit, hitTransform);
            RaycastHit snappedHit = GetSnappedHit(localisedHit);
            RaycastHit snappedWorldSpaceHit = GetWorldSpaceHit(snappedHit, hitTransform);
            RaycastHit poppedHit = PopHitOntoBestSurface(snappedWorldSpaceHit);

            Cache.SnapBuilderAimTransform.position = Cache.OffsetAimTransform.position;
            Cache.SnapBuilderAimTransform.forward = poppedHit.point - Cache.SnapBuilderAimTransform.position;

            return Cache.SnapBuilderAimTransform;
        }

        public static bool IsColliderImproved() => IsColliderImproved(Cache.LastCollider);
        public static bool IsColliderImprovable() => IsColliderImprovable(Cache.LastCollider, out Mesh _);

        private static Type[] ExcludedComponentTypes { get; } = new Type[] {
            typeof(CoralBlendWhite) // default mesh is good enough and doesn't work well when upgraded
        };

        private static bool IsExcluded(Transform transform)
        {
            foreach (var componentType in ExcludedComponentTypes)
            {
                if (!(transform.GetComponent(componentType) is null))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsColliderImproved(Collider collider)
            => collider is Collider
               && Cache.IsImprovedByCollider.TryGetValue(collider, out bool isImproved)
               && isImproved;

        private static bool IsColliderImprovable(Collider collider, out Mesh mesh)
        {
            mesh = null;

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh is Mesh)
            {
                if (Cache.ImprovedMeshByCollider.TryGetValue(meshCollider, out mesh))
                {
                    return true;
                }

                Transform rootTransform = UWE.Utils.GetEntityRoot(collider.transform.gameObject)?.transform ?? collider.transform;
                IEnumerable<Mesh> potentialMeshes = rootTransform.GetComponentsInChildren<MeshFilter>()
                    .Where(x => !IsExcluded(rootTransform))
                    .Where(meshFilter => meshFilter is MeshFilter)
                    .Select(meshFilter => meshFilter.sharedMesh)
                    .AddItem(meshCollider.sharedMesh)
                    .Where(mesh => mesh is Mesh && mesh.isReadable && (!meshCollider.convex || mesh.triangles.Count() / 3 <= 255))
                    .Distinct()
                    .OrderByDescending(mesh => mesh.triangles.Count());

                mesh = potentialMeshes.FirstOrDefault();

                if (mesh is Mesh && mesh != meshCollider.sharedMesh)
                {
                    Cache.ImprovedMeshByCollider[meshCollider] = mesh;
                    return true;
                }
            }

            return false;
        }

        private static bool TryImproveCollider(RaycastHit hit)
        {
            if (IsColliderImprovable(hit.collider, out Mesh mesh))
            {
                if (hit.collider is MeshCollider meshCollider)
                {
                    Cache.OriginalMeshByCollider[hit.collider] = meshCollider.sharedMesh;
                    SetMesh(meshCollider, mesh);
                }
                else
                {
                    // code for upgrading box colliders etc. goes here...
                }

                Cache.IsImprovedByCollider[hit.collider] = true;
                Logger.LogWarning("Upgraded mesh");

                return true;
            }

            return false;
        }

        private static bool TryRevertCollider(Collider collider)
        {
            bool reverted = false;

            if (!Cache.IsImprovedByCollider.TryGetValue(collider, out bool isImproved) || !isImproved)
            {
                return false;
            }

            if (collider is MeshCollider meshCollider)
            {
                if (Cache.OriginalMeshByCollider.TryGetValue(meshCollider, out Mesh originalMesh))
                {
                    SetMesh(meshCollider, originalMesh);
                    reverted = true;
                }
            }
            else
            {
                // code for reverting box colliders etc. goes here...
            }

            if (reverted)
            {
                Cache.IsImprovedByCollider[collider] = false;
                Logger.LogWarning("Reverted mesh");
            }

            return reverted;
        }
        private static bool TryRevertCollider(RaycastHit hit) => TryRevertCollider(hit.collider);

        private static void ImproveColliderAndUpdateHit(ref RaycastHit hit)
        {
            if (TryImproveCollider(hit)
                && !Physics.Raycast(Cache.OffsetAimTransform.position,
                            Cache.BuilderAimTransform.forward,
                            out hit,
                            Builder.placeMaxDistance,
                            Builder.placeLayerMask,
                            QueryTriggerInteraction.Ignore))
            {
                Logger.LogWarning("couldn't get a new hit...");
            }
        }

        private static void RevertColliderAndUpdateHit(ref RaycastHit hit)
        {
            if (TryRevertCollider(hit)
                && !Physics.Raycast(Cache.OffsetAimTransform.position,
                            Cache.BuilderAimTransform.forward,
                            out hit,
                            Builder.placeMaxDistance,
                            Builder.placeLayerMask,
                            QueryTriggerInteraction.Ignore))
            {
                Logger.LogWarning("couldn't get a new hit...");
            }
        }

        public static void RevertColliders()
        {
            Cache.LastCollider = null;
            foreach (Collider collider in Cache.OriginalMeshByCollider.Keys)
            {
                TryRevertCollider(collider);
            }
        }

        private static IEnumerator DestroyNextFrame(GameObject gameObject)
        {
            yield return null;
            GameObject.Destroy(gameObject);
        }

        private static void SetMesh(MeshCollider meshCollider, Mesh mesh)
        {
            meshCollider.sharedMesh = mesh;
        }

        private static void RenderCollider(Collider collider, Material material, float scale = 1f)
        {
            if (collider is null)
                return;

            var gameObject = new GameObject("collider renderer");
            switch (collider)
            {
                case MeshCollider meshCollider:
                    gameObject.AddComponent<MeshFilter>().mesh = meshCollider.sharedMesh;
                    var renderer = gameObject.AddComponent<MeshRenderer>();

                    renderer.sharedMaterial = material;
                    break;
                default:
                    var primitive = GameObject.CreatePrimitive(collider switch
                    {
                        BoxCollider _ => PrimitiveType.Cube,
                        SphereCollider _ => PrimitiveType.Sphere,
                        CapsuleCollider _ => PrimitiveType.Capsule,
                        _ => throw new NotImplementedException()
                    });
                    if (primitive.GetComponent<Collider>() is Collider primitiveCollider)
                        primitiveCollider.enabled = false;
                    primitive.GetComponent<Renderer>().material = material;
                    primitive.transform.SetParent(gameObject.transform, false);
                    break;
            }
            gameObject.transform.SetParent(collider.transform, false);
            gameObject.transform.localScale = collider.transform.localScale * scale;
            UWE.CoroutineHost.StartCoroutine(DestroyNextFrame(gameObject));
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
