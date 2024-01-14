using System.Collections.Generic;
using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class RuntimeHelper
{
    public static void SetLocalPosY(this RectTransform rectTransform, float y)
    {
        var localPos = rectTransform.localPosition;
        rectTransform.localPosition = new Vector3(localPos.x, y, localPos.z);
    }

    public static void SetPivotY(this RectTransform rectTransform, float y)
    {
        rectTransform.pivot = new Vector2(rectTransform.pivot.x, y);
    }

    public static void SetAnchorMinY(this RectTransform rectTransform, float y)
    {
        rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, y);
    }

    public static void SetAnchorMaxY(this RectTransform rectTransform, float y)
    {
        rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, y);
    }

    public static void SetAnchoredPosY(this RectTransform rectTransform, float y)
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
    }
    
    public static void SetSizeDeltaX(this RectTransform rectTransform, float x)
    {
        rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
    }

    public static Rect UiBoundsWorld(this RectTransform rectTransform)
    {
        var position = rectTransform.position;
        var rect = rectTransform.rect;
        var scale = rectTransform.lossyScale;

        return new Rect((rect.x * scale.x) + position.x, (rect.y * scale.y) + position.y, rect.width * scale.x, rect.height * scale.y);
    }
    
    public static Rect UiBounds(this RectTransform rectTransform)
    {
        var rect = rectTransform.rect;
        var scale = rectTransform.lossyScale;

        return new Rect(rect.x * scale.x, rect.y * scale.y, rect.width * scale.x, rect.height * scale.y);
    }
    
    public static Rect UiBounds(this RectTransform rectTransform, Vector3 position)
    {
        var rect = rectTransform.rect;
        var scale = rectTransform.lossyScale;

        return new Rect((rect.x * scale.x) + position.x, (rect.y * scale.y) + position.y, rect.width * scale.x, rect.height * scale.y);
    }

    public static float WorldMaxY(this RectTransform rectTransform)
    {
        return rectTransform.UiBoundsWorld().max.y;
    }
    
    public static float WorldMinY(this RectTransform rectTransform)
    {
        return rectTransform.UiBoundsWorld().min.y;
    }

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