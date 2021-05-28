using System;
using System.Collections;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.ExtensionMethods
{
    using UnityEngine;
    using UWE;

    internal static class UnityEngineExtensionMethods
    {
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

        public static IEnumerator DestroyNextFrame(this Object obj)
        {
            yield return null;
            Object.Destroy(obj);
        }
    }
}
