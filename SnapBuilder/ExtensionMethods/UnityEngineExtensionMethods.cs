using System;
using System.Collections;
using System.Collections.Generic;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.ExtensionMethods
{
    using UnityEngine;
    using UWE;

    internal static class UnityEngineExtensionMethods
    {
        #region Object
        public static IEnumerator DestroyNextFrame(this Object obj)
        {
            yield return null;
            Object.Destroy(obj);
        }
        #endregion

        #region Collider
        public static void Render(this Collider collider, Material material, float scale = 1f)
        {
            if (collider is null || collider == null)
            {
                return;
            }

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
                    {
                        primitiveCollider.enabled = false;
                    }

                    primitive.GetComponent<Renderer>().material = material;
                    primitive.transform.SetParent(gameObject.transform, false);
                    break;
            }
            gameObject.transform.SetParent(collider.transform, false);
            gameObject.transform.localScale = collider.transform.localScale * scale;
            CoroutineHost.StartCoroutine(gameObject.DestroyNextFrame());
        }
        #endregion

        #region RaycastHit
        /// <summary>
        /// Where possible, use the transform of the parent as this should be a better reference point for localisation
        /// (especially useful inside a base)
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static Transform GetOptimalTransform(this RaycastHit hit) => Builder.GetSurfaceType(hit.normal) switch
        {
            SurfaceType.Ground => hit.transform.parent ?? hit.transform,
            _ => hit.transform
        };

        /// <summary>
        /// Where appropriate, returns a recalculated hit.normal direction by performing a number of new hits around the same area
        /// as the original, and averaging the normals
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Vector3 AverageNormal(this RaycastHit hit, float scale = 1f)
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
                    AimTransform.Raycast(hit.point + Vector3.up * .1f + offset * scale,
                            Vector3.down,
                            out RaycastHit offsetHit);
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
            return hit.normal;
        }
        #endregion
    }
}
