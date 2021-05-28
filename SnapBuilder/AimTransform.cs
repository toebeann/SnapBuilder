using Straitjacket.ExtensionMethods.UnityEngine;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal class AimTransform : MonoBehaviour
    {
        private static AimTransform main;
        public static AimTransform Main => main == null
            ? new GameObject("SnapBuilder").AddComponent<AimTransform>()
            : main;

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
