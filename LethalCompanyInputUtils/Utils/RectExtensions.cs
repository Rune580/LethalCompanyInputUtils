using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class RectExtensions
{
    public static void Deconstruct(this Rect rect, out Vector2 min, out Vector2 max)
    {
        min = new Vector2(rect.xMin, rect.yMin);
        max = new Vector2(rect.xMax, rect.yMax);
    }
}