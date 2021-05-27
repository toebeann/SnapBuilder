using Straitjacket.ExtensionMethods.UnityEngine;
using System.Collections.Generic;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    using SceneManagement;

    internal class Cache
    {
        /// <summary>
        /// The camera transform, as per the original Builder.GetAimTransform()
        /// </summary>
        public Transform BuilderAimTransform => MainCamera.camera.transform;
        public const string SnapBuilderAimTransformName = "SnapBuilderAimTransform";

        private SceneCache<Transform> snapBuilderAimTransformCache;
        /// <summary>
        /// The transform attached to our custom GameObject for snapped aiming
        /// </summary>
        public Transform SnapBuilderAimTransform
        {
            get
            {
                snapBuilderAimTransformCache ??= new SceneCache<Transform>();
                snapBuilderAimTransformCache.Data ??= BuilderAimTransform.Find(SnapBuilderAimTransformName);
                if (snapBuilderAimTransformCache.Data is null)
                {
                    snapBuilderAimTransformCache.Data = new GameObject(SnapBuilderAimTransformName).transform;
                    snapBuilderAimTransformCache.Data.SetParent(BuilderAimTransform, false);
                }

                return snapBuilderAimTransformCache.Data;
            }
        }

        private SceneCache<Transform> offsetAimTransformCache;
        /// <summary>
        /// A non-moving parent of the MainCamera transform, to counteract head-bobbing
        /// </summary>
        public Transform OffsetAimTransform
        {
            get
            {
                offsetAimTransformCache ??= new SceneCache<Transform>();
                return offsetAimTransformCache.Data ??= BuilderAimTransform.FindAncestor("camOffset").parent
                        ?? BuilderAimTransform.FindAncestor(transform => !transform.position.Equals(OffsetAimTransform.position))
                        ?? BuilderAimTransform;
            }
        }

        private SceneCache<Dictionary<Collider, Mesh>> originalMeshByColliderCache;
        /// <summary>
        /// A dictionary of colliders mapped to their original mesh
        /// </summary>
        public Dictionary<Collider, Mesh> OriginalMeshByCollider
        {
            get
            {
                originalMeshByColliderCache ??= new SceneCache<Dictionary<Collider, Mesh>>();
                return originalMeshByColliderCache.Data ??= new Dictionary<Collider, Mesh>();
            }
        }

        private SceneCache<Dictionary<Collider, Mesh>> improvedMeshByColliderCache;
        /// <summary>
        /// A dictionary of colliders mapped to the map chosen to replace the original
        /// </summary>
        public Dictionary<Collider, Mesh> ImprovedMeshByCollider
        {
            get
            {
                improvedMeshByColliderCache ??= new SceneCache<Dictionary<Collider, Mesh>>();
                return improvedMeshByColliderCache.Data ??= new Dictionary<Collider, Mesh>();
            }
        }

        private SceneCache<Dictionary<Collider, bool>> isImprovedByColliderCache;
        /// <summary>
        /// A dictionary of collider mapped to whether they are currently improved
        /// </summary>
        public Dictionary<Collider, bool> IsImprovedByCollider
        {
            get
            {
                isImprovedByColliderCache ??= new SceneCache<Dictionary<Collider, bool>>();
                return isImprovedByColliderCache.Data ??= new Dictionary<Collider, bool>();
            }
        }

        private SceneCache<Material> colliderMaterialCache;
        public Material ColliderMaterial
        {
            get
            {
                colliderMaterialCache ??= new SceneCache<Material>();
                return colliderMaterialCache.Data ??= new Material(Builder.ghostStructureMaterial);
            }
        }

        private SceneCache<Collider> lastColliderCache;
        /// <summary>
        /// The Collider that the player most recently aimed at, or null.
        /// </summary>
        public Collider LastCollider
        {
            get => (lastColliderCache ??= new SceneCache<Collider>()).Data;
            set => (lastColliderCache ??= new SceneCache<Collider>()).Data = value;
        }
    }
}
