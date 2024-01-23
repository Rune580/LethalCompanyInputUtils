using System;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Components.PopOvers;

[RequireComponent(typeof(RectTransform))]
public class PopOver : MonoBehaviour
{
    public RectTransform? popOverLayer;
    public PopOverTextContainer? textContainer;
    public RectTransform? pivotPoint;
    public GameObject? background;
    public PopOverArrow? arrow;
    public CanvasGroup? canvasGroup;
    public float maxWidth = 300f;

    private RectTransform? _rectTransform;
    private RectTransform? _target;
    private Placement _placement;

    public void SetTarget(RectTransform target, Placement placement)
    {
        if (background is null || canvasGroup is null)
            return;
        
        background.SetActive(true);
        
        _target = target;
        _placement = placement;

        SetPivot();
        SetArrow();
    }

    public void ClearTarget()
    {
        if (background is null || canvasGroup is null)
            return;

        canvasGroup.alpha = 0;
        
        _target = null;
        background.SetActive(false);
    }

    private void MoveTo(RectTransform target)
    {
        if (pivotPoint is null)
            return;

        var targetPos = (Vector3)GetTargetPosition(target);
        var offset = (Vector3)GetTargetPivotOffset(target);
        var labelOffset = (Vector3)GetLabelPivotOffset();

        var pivotTarget = targetPos + offset + labelOffset;
        
        MovePopOverToTarget(pivotTarget);
        AdjustArrowPosToTarget(target);
    }

    private void SetPivot()
    {
        if (pivotPoint is null)
            return;

        pivotPoint.pivot = _placement switch
        {
            Placement.Top => new Vector2(0.5f, 0f),
            Placement.Bottom => new Vector2(0.5f, 1f),
            Placement.Left => new Vector2(1f, 0.5f),
            Placement.Right => new Vector2(0f, 0.5f),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void SetArrow()
    {
        if (arrow is null)
            return;

        switch (_placement)
        {
            case Placement.Top:
                arrow.PointToBottom();
                break;
            case Placement.Bottom:
                arrow.PointToTop();
                break;
            case Placement.Left:
                arrow.PointToRight();
                break;
            case Placement.Right:
                arrow.PointToLeft();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Vector2 GetTargetPosition(RectTransform target)
    {
        if (popOverLayer is null)
            return Vector2.zero;

        return popOverLayer.WorldToLocalPoint(target);
    }

    private Vector2 GetTargetPivotOffset(RectTransform target)
    {
        if (popOverLayer is null)
            return Vector2.zero;
        
        var targetRect = popOverLayer.GetRelativeRect(target);
        const float offset = 2f;

        return _placement switch {
            Placement.Top => new Vector2(0f, offset + targetRect.height / 2f),
            Placement.Bottom => new Vector2(0f, -offset + -targetRect.height / 2f),
            Placement.Left => new Vector2(-offset + -targetRect.width / 2f, 0f),
            Placement.Right => new Vector2(offset + targetRect.width / 2f, 0f),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Vector2 GetLabelPivotOffset()
    {
        if (textContainer is null || textContainer.rectTransform is null || _rectTransform is null || popOverLayer is null)
            return Vector2.zero;

        var rect = popOverLayer.GetRelativeRect(_rectTransform);
        var textRect = popOverLayer.GetRelativeRect(textContainer.rectTransform);

        return _placement switch
        {
            Placement.Top => new Vector2(0f, -((rect.height - textRect.height) / 2f)),
            Placement.Bottom => new Vector2(0f, (rect.height - textRect.height) / 2f),
            Placement.Left => new Vector2((rect.width - textRect.width) / 2f, 0f),
            Placement.Right => new Vector2(-((rect.width - textRect.width) / 2f), 0f),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private void MovePopOverToTarget(Vector3 targetPos)
    {
        if (pivotPoint is null || popOverLayer is null || _rectTransform is null)
            return;

        var view = popOverLayer.rect;

        var diff = targetPos - pivotPoint.localPosition;
        var movement = new Vector2(diff.x, diff.y);
        
        var popOverRect =  popOverLayer.GetRelativeRect(_rectTransform);
        var nextMax = popOverRect.max + movement;
        var nextMin = popOverRect.min + movement;
        
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

        pivotPoint.localPosition += (Vector3)movement;
    }
    
    private void AdjustArrowPosToTarget(RectTransform target)
    {
        if (arrow is null)
            return;
        
        arrow.SetTargetPos(target);
    }

    public void SetText(string text)
    {
        if (textContainer is null)
            return;
        
        textContainer.SetText(text);
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.SetSizeDeltaX(maxWidth);
        
        SetText("");
    }

    private void Update()
    {
        if (_target is null)
            return;
        
        MoveTo(_target);
    }

    private void LateUpdate()
    {
        if (_target is null || canvasGroup is null)
            return;
        
        if (canvasGroup.alpha == 0)
            canvasGroup.alpha = 1;
    }

    public enum Placement
    {
        Top,
        Bottom,
        Left,
        Right
    }
}