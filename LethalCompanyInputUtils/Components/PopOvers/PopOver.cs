using System;
using TMPro;
using UnityEngine;

namespace LethalCompanyInputUtils.Components.PopOvers;

public class PopOver : MonoBehaviour
{
    public RectTransform? popOverLayer;
    public TextMeshProUGUI? label;
    public RectTransform? pivotPoint;

    private RectTransform? _target;
    private Placement _placement;

    public void SetTarget(RectTransform target, Placement placement)
    {
        ClearTarget();
        
        _target = target;
        _placement = placement;

        SetPivot();
    }

    public void ClearTarget()
    {
        _target = null;
        SetLabel("");
    }

    private void MoveTo(RectTransform target)
    {
        if (pivotPoint is null)
            return;

        var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.current, target.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(popOverLayer, screenPoint, Camera.current, out var localPoint))
            return;
        
        pivotPoint.localPosition = localPoint;
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

    public void SetLabel(string text)
    {
        if (label is null)
            return;
        
        label.SetText(text);
    }

    private void Awake()
    {
        SetLabel("");
    }

    private void Update()
    {
        if (_target is null)
            return;
        
        MoveTo(_target);
    }

    public enum Placement
    {
        Top,
        Bottom,
        Left,
        Right
    }
}