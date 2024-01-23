using System.Collections.Generic;
using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class RuntimeHelper
{
    public static Vector3 LocalPositionRelativeTo(this Transform transform, Transform parent)
    {
        var totalOffset = Vector3.zero;

        Transform tempParent = transform;
        do
        {
            totalOffset += transform.localPosition;
            tempParent = tempParent.parent;
        } while (tempParent != parent);


        return totalOffset;
    }

    public static void DisableKeys(this IEnumerable<RemappableKey> keys)
    {
        foreach (var key in keys)
            key.currentInput.action.Disable();
    }
    
    public static void EnableKeys(this IEnumerable<RemappableKey> keys)
    {
        foreach (var key in keys)
            key.currentInput.action.Enable();
    }
}