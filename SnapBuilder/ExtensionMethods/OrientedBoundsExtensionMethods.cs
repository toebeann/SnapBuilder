using System.Collections.Generic;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder.ExtensionMethods
{
    internal static class OrientedBoundsExtensionMethods
    {
        public static List<Vector3> GetCorners(this OrientedBounds bounds)
        {
            List<Vector3> corners = new List<Vector3>();
            corners.Add(bounds.position - bounds.extents);
            corners.Add(bounds.position + bounds.extents);
            corners.Add(new Vector3(corners[0].x, corners[0].y, corners[1].z));
            corners.Add(new Vector3(corners[0].x, corners[1].y, corners[0].z));
            corners.Add(new Vector3(corners[1].x, corners[0].y, corners[0].z));
            corners.Add(new Vector3(corners[0].x, corners[1].y, corners[1].z));
            corners.Add(new Vector3(corners[1].x, corners[1].y, corners[0].z));
            corners.Add(new Vector3(corners[1].x, corners[0].y, corners[1].z));
            return corners;
        }
    }
}
