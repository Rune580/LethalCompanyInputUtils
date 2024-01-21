using UnityEngine;

namespace LethalCompanyInputUtils.Components.PopOvers;

[RequireComponent(typeof(RectTransform))]
public class PopOverArrow : MonoBehaviour
{
    public RectTransform? parent;
    public RectTransform? rectTransform;

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

    public void SetTargetPos(Vector3 targetPos)
    {
        if (rectTransform is null)
            return;
        
        var pos = rectTransform.position;

        if (!_lockX)
            pos = new Vector3(targetPos.x, pos.y, pos.z);

        if (!_lockY)
            pos = new Vector3(pos.x, targetPos.y, pos.z);

        rectTransform.position = pos;
    }
}