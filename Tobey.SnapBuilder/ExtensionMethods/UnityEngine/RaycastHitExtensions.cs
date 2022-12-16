using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobey.SnapBuilder.ExtensionMethods.UnityEngine;
internal static class RaycastHitExtensions
{
    /// <summary>
    /// Where possible, use the transform of the parent as this should be a better reference point for localisation
    /// (especially useful inside a base)
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    public static Transform GetOptimalTransform(this RaycastHit hit)
    {
        var surfaceType = Builder.GetSurfaceType(hit.normal);

        if (surfaceType == SurfaceType.Ground)
        {
            if (UWE.Utils.GetEntityRoot(hit.transform.gameObject)?.transform is Transform root)
            {
                if (root.GetComponent<BaseCell>() != null)
                {
                    return root;
                }
                else if (root.GetComponent<Base>() != null)
                {
                    return hit.transform;
                }
                else if (new float[] { 1, 0, 1 / Mathf.Sqrt(2) }
                    .Any(dot => Mathf.Approximately(dot, Mathf.Abs(Vector3.Dot(root.forward, hit.transform.forward)))))
                {
                    return root;
                }
            }

            if (hit.transform.parent is Transform parent)
            {
                if (parent.GetComponent<BaseCell>() != null)
                {
                    return parent;
                }
                else if (parent.GetComponent<Base>() != null)
                {
                    return hit.transform;
                }
                else if (new float[] { 1, 0, 1 / Mathf.Sqrt(2) }
                    .Any(dot => Mathf.Approximately(dot, Mathf.Abs(Vector3.Dot(parent.forward, hit.transform.forward)))))
                {
                    return parent;
                }
            }
        }

        return hit.transform;
    }

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

            HashSet<Vector3> normals = new();
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
}
