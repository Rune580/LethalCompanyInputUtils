using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Components.PopOvers;

[RequireComponent(typeof(RectTransform))]
public class PopOverArrow : MonoBehaviour
{
    public PopOver? popOver;
    public RectTransform? parent;
    public RectTransform? rectTransform;
    public Vector2 arrowPadding = new(2f, 2f);

    private bool _lockX;
    private bool _lockY;

    private void Awake()
    {
        if (rectTransform is null)
            rectTransform = GetComponent<RectTransform>();

        if (parent is null)
            parent = GetComponentInParent<RectTransform>();

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

        _lockX = false;
        _lockY = true;
    }

    public void PointToTop()
    {
        if (rectTransform is null)
            return;

        rectTransform.anchoredPosition = new Vector2(0f, 0.2f);
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.eulerAngles = new Vector3(0, 0, 90f);

        _lockX = false;
        _lockY = true;
    }

    public void PointToRight()
    {
        if (rectTransform is null)
            return;

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(1f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.eulerAngles = new Vector3(0, 0, 0);
        
        _lockX = true;
        _lockY = false;
    }
    
    public void PointToLeft()
    {
        if (rectTransform is null)
            return;

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.eulerAngles = new Vector3(0, 0, 180f);
        
        _lockX = true;
        _lockY = false;
    }

    public void SetTargetPos(RectTransform target)
    {
        if (rectTransform is null || parent is null || popOver is null || popOver.popOverLayer is null)
            return;

        var targetRect = popOver.popOverLayer.GetRelativeRect(target);
        var arrowRect = popOver.popOverLayer.GetRelativeRect(rectTransform);

        var diff = targetRect.CenteredPos() - arrowRect.CenteredPos();
        var movement = diff;

        var view = popOver.popOverLayer.GetRelativeRect(parent);
        
        var nextMin = (arrowRect.min - arrowPadding) + movement;
        var nextMax = (arrowRect.max + arrowPadding) + movement;
        
        if (nextMax.x > view.xMax)
        {
            var xOffset = nextMax.x - view.xMax;
            movement = new Vector2(movement.x - xOffset, movement.y);
        }
        if (nextMin.x < view.xMin)
        {
            var xOffset = view.xMin - nextMin.x;
            movement = new Vector2(movement.x + xOffset, movement.y);
        }
        if (nextMax.y > view.yMax)
        {
            var yOffset = nextMax.y - view.yMax;
            movement = new Vector2(movement.x, movement.y - yOffset);
        }
        if (nextMin.y < view.yMin)
        {
            var yOffset = view.yMin - nextMin.y;
            movement = new Vector2(movement.x, movement.y + yOffset);
        }

        if (_lockX)
            movement = new Vector2(0, movement.y);
        if (_lockY)
            movement = new Vector2(movement.x, 0);

        rectTransform.localPosition += (Vector3)movement;
    }
}