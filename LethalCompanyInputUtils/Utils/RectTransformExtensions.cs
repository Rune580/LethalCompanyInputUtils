using System;
using UnityEngine;

namespace LethalCompanyInputUtils.Utils;

internal static class RectTransformExtensions
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

    [Obsolete("Use GetRelativeRect")]
    public static Rect UiBoundsWorld(this RectTransform rectTransform)
    {
        var position = rectTransform.position;
        var rect = rectTransform.rect;
        var scale = rectTransform.lossyScale;

        return new Rect((rect.x * scale.x) + position.x, (rect.y * scale.y) + position.y, rect.width * scale.x, rect.height * scale.y);
    }

    [Obsolete("Use GetRelativeRect")]
    public static Rect UiBounds(this RectTransform rectTransform, Vector3 position)
    {
        var rect = rectTransform.rect;
        var scale = rectTransform.lossyScale;

        return new Rect((rect.x * scale.x) + position.x, (rect.y * scale.y) + position.y, rect.width * scale.x, rect.height * scale.y);
    }

    public static Rect GetRelativeRect(this RectTransform rectTransform, RectTransform worldRectTransform)
    {
        var camera = CameraUtils.GetBestUiCamera();
        var corners = new Vector3[4];
        worldRectTransform.GetWorldCorners(corners);

        var screenCorners = new Vector2[4];
        for (int i = 0; i < corners.Length; i++)
            screenCorners[i] = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);

        var localCorners = new Vector2[4];
        for (int i = 0; i < screenCorners.Length; i++)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenCorners[i], camera,
                out localCorners[i]);
        }

        var min = localCorners[0];
        var max = localCorners[0];

        foreach (var corner in localCorners)
        {
            min = Vector2.Min(min, corner);
            max = Vector2.Max(max, corner);
        }

        var size = max - min;
        return new Rect(min.x, min.y, size.x, size.y);
    }

    public static float WorldMaxY(this RectTransform rectTransform)
    {
        return rectTransform.UiBoundsWorld().max.y;
    }

    public static float WorldMinY(this RectTransform rectTransform)
    {
        return rectTransform.UiBoundsWorld().min.y;
    }
}