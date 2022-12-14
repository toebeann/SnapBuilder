using System;
using UnityEngine;

namespace Tobey.SnapBuilder.ExtensionMethods.UnityEngine;
internal static class HierarchyExtensions
{
    public static Transform FindAncestor(this Transform current, string name)
    {
        if (current.parent == null || current.parent.name == name)
            return current.parent;
        return current.parent.FindAncestor(name);
    }

    public static Transform FindAncestor(this Transform current, Func<Transform, bool> filter)
    {
        if (current.parent == null || filter(current.parent))
            return current.parent;
        return current.parent.FindAncestor(filter);
    }
}
