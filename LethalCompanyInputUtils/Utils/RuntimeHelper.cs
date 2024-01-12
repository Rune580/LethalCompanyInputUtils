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

    public static float WorldCornersMaxY(this RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        
        rectTransform.GetWorldCorners(corners);

        return corners[1].y;
    }
    
    public static float WorldCornersMinY(this RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        
        rectTransform.GetWorldCorners(corners);

        return corners[0].y;
    }
}