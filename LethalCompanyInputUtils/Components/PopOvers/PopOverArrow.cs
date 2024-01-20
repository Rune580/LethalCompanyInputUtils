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

        rectTransform.drivenProperties = DrivenTransformProperties.Anchors | DrivenTransformProperties.Rotation |
                                         DrivenTransformProperties.AnchoredPosition;
        rectTransform.drivenByObject = this;
    }

    public void PointToBottom()
    {
        if (rectTransform is null)
            return;

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.eulerAngles = new Vector3(0, 0, -90f);
    }

    public void PointToTop()
    {
        if (rectTransform is null)
            return;

        rectTransform.anchoredPosition = new Vector2(0f, 0.2f);
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.eulerAngles = new Vector3(0, 0, 90f);
    }

    public void PointToRight()
    {
        if (rectTransform is null)
            return;

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(1f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.eulerAngles = new Vector3(0, 0, 0);
    }
    
    public void PointToLeft()
    {
        if (rectTransform is null)
            return;

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.eulerAngles = new Vector3(0, 0, 180f);
    }

    public void SetXTarget(float x)
    {
        if (rectTransform is null)
            return;
        
        rectTransform.SetAnchoredPosX(x);
    }
    
    public void SetYTarget(float y)
    {
        if (rectTransform is null)
            return;
        
        rectTransform.SetAnchoredPosY(y);
    }
}