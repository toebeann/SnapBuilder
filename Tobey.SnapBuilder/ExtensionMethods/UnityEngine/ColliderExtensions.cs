using System;
using UnityEngine;
using UWE;

namespace Tobey.SnapBuilder.ExtensionMethods.UnityEngine;
internal static class ColliderExtensions
{
    public static void Render(this Collider collider, Material material, float scale = 1f)
    {
        if (collider == null)
        {
            return;
        }

        var gameObject = new GameObject("collider renderer");
        switch (collider)
        {
            case MeshCollider meshCollider:
                gameObject.AddComponent<MeshFilter>().mesh = meshCollider.sharedMesh;
                var meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;
                break;

            default:
                var primitive = GameObject.CreatePrimitive(collider switch
                {
                    BoxCollider => PrimitiveType.Cube,
                    SphereCollider => PrimitiveType.Sphere,
                    CapsuleCollider => PrimitiveType.Capsule,
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
}
