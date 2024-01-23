using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class RectUtils
{
    public static Vector2 CenteredPos(this Rect rect) => rect.min + rect.size / 2f;
}