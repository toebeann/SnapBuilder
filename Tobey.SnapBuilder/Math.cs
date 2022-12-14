using UnityEngine;

namespace Tobey.SnapBuilder;
internal static class Math
{
    public static float RoundToNearest(float x, float y) => y * Mathf.Round(x / y);

    public static float FloorToNearest(float x, float y) => y * Mathf.Floor(x / y);
}
