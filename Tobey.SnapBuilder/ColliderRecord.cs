using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobey.SnapBuilder;
using ExtensionMethods.UnityEngine;
public class ColliderRecord
{
    private static Type[] ExcludedComponentTypes { get; } = new Type[] {
            typeof(CoralBlendWhite), // default mesh is good enough and doesn't work well when upgraded
            typeof(BaseCell),
            typeof(ConstructableBase)
        };

    private static TechType[] ExcludedTechTypes { get; } = new TechType[]
    {
            TechType.BaseHatch
    };

    private static Material material;
    public static Material Material => material ??= new Material(Builder.ghostStructureMaterial);

    public Collider Collider { get; }
    private bool? isImprovable;
    public bool IsImprovable
    {
        get
        {
            if (isImprovable is bool improvable)
            {
                return improvable;
            }

            if (ImprovedMesh is Mesh)
            {
                return (isImprovable = true).Value;
            }

            if (IsExcluded())
            {
                return (isImprovable = false).Value;
            }

            if (Collider is MeshCollider meshCollider && meshCollider.sharedMesh is Mesh)
            {
                OriginalMesh ??= meshCollider.sharedMesh;

                Transform root = UWE.Utils.GetEntityRoot(Collider.transform.gameObject)?.transform ?? Collider.transform;
                IEnumerable<Mesh> potentialMeshes = root.GetComponentsInChildren<MeshFilter>()
                    .Where(filter => filter is MeshFilter)
                    .Select(filter => filter.sharedMesh)
                    .Concat(new[] { meshCollider.sharedMesh })
                    .Where(mesh => mesh is Mesh && mesh.isReadable && (!meshCollider.convex || mesh.triangles.Count() / 3 <= 255))
                    .Distinct()
                    .OrderByDescending(mesh => mesh.triangles.Count());

                Mesh mesh = potentialMeshes.FirstOrDefault();
                if (mesh is Mesh && mesh != meshCollider.sharedMesh)
                {
                    ImprovedMesh = mesh;
                    return (isImprovable = true).Value;
                }
            }

            return (isImprovable = false).Value;
        }
    }
    public bool IsImproved => IsImprovable && Collider switch
    {
        MeshCollider meshCollider when meshCollider != null => meshCollider.sharedMesh is Mesh mesh && mesh == ImprovedMesh,
        _ => false
    };
    public Mesh OriginalMesh { get; private set; }
    public Mesh ImprovedMesh { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public ColliderRecord(Collider collider)
    {
        Collider = collider;
    }

    private bool IsExcluded()
    {
        Transform root = UWE.Utils.GetEntityRoot(Collider.transform.gameObject)?.transform ?? Collider.transform;
        return ExcludedTechTypes.Contains(Builder.constructableTechType)
               || ExcludedComponentTypes
                      .Select(type => root.GetComponent(type))
                      .Any(component => component is Component);
    }

    public void Improve()
    {
        if (IsImprovable && !IsImproved)
        {
            if (Collider is MeshCollider meshCollider && ImprovedMesh is Mesh)
            {
                meshCollider.sharedMesh = ImprovedMesh;
            }
        }
    }

    public void Revert()
    {
        if (IsImprovable && IsImproved)
        {
            if (Collider is MeshCollider meshCollider && OriginalMesh is Mesh)
            {
                meshCollider.sharedMesh = OriginalMesh;
                Timestamp = DateTime.UtcNow;
            }
        }
    }

    public void Render()
    {
        Material.SetColor(ShaderPropertyID._Tint, IsImproved ? Color.black : Color.gray);
        Collider.Render(Material, 1.00001f);
    }
}
