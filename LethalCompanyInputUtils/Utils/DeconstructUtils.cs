using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class DeconstructUtils
{
    public static void Deconstruct(this Rect rect, out Vector2 min, out Vector2 max)
    {
        min = new Vector2(rect.xMin, rect.yMin);
        max = new Vector2(rect.xMax, rect.yMax);
    }

    public static void Deconstruct(this Vector3 vector, out float x, out float y, out float z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}