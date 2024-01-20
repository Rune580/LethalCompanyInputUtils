using System;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Components.PopOvers;

[RequireComponent(typeof(RectTransform))]
public class PopOverArrow : MonoBehaviour
{
    public RectTransform? rectTransform;

    private void Awake()
    {
        if (rectTransform is null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.drivenProperties = DrivenTransformProperties.Anchors | DrivenTransformProperties.Rotation;
        rectTransform.drivenByObject = this;
    }

    public void PointToBottom()
    {
        if (rectTransform is null)
            return;

        rectTransform.SetAnchoredPosY(0f);
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.eulerAngles = new Vector3(0, 0, -90f);
    }

    public void PointToTop()
    {
        if (rectTransform is null)
            return;

        rectTransform.SetAnchoredPosY(0.2f);
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.eulerAngles = new Vector3(0, 0, 90f);
    }

    public void PointToRight()
    {
        if (rectTransform is null)
            return;

        rectTransform.SetAnchoredPosY(0f);
        rectTransform.anchorMin = new Vector2(1f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.eulerAngles = new Vector3(0, 0, 0);
    }
    
    public void PointToLeft()
    {
        if (rectTransform is null)
            return;

        rectTransform.SetAnchoredPosY(0f);
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.eulerAngles = new Vector3(0, 0, 180f);
    }
}