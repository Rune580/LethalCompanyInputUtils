using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class VectorUtils
{
    public static Vector3 Mul(this Vector3 left, Vector3 right) =>
        new(left.x * right.x, left.y * right.y, left.z * right.z);
}