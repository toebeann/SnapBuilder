using System.Collections;
using UnityEngine;

namespace Tobey.SnapBuilder.ExtensionMethods.UnityEngine;
internal static class ObjectExtensions
{
    public static IEnumerator DestroyNextFrame(this Object obj)
    {
        yield return null;
        Object.Destroy(obj);
    }
}
