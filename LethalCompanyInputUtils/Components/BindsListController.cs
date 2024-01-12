using System.Collections.Generic;
using LethalCompanyInputUtils.Components.Section;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components;

[RequireComponent(typeof(RectTransform))]
public class BindsListController : MonoBehaviour
{
    public GameObject? sectionHeaderPrefab;
    public GameObject? sectionAnchorPrefab;
    public GameObject? rebindItemPrefab;
    public GameObject? spacerPrefab;
    
    public ScrollRect? scrollRect;
    public RectTransform? headerContainer;
    
    public UnityEvent<int> OnSectionChanged = new();

    private RectTransform? _rectTransform;
    private RectTransform? _scrollRectTransform;
    private RectTransform? _content;
    private VerticalLayoutGroup? _verticalLayoutGroup;

    private int _currentSection;
    private float _sectionHeight;
    private float _spacing;

    private readonly List<SectionHeaderAnchor> _anchors = [];

    private void Awake()
    {
        if (_rectTransform is null)
            _rectTransform = GetComponent<RectTransform>();
        
        if (scrollRect is null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        if (_verticalLayoutGroup is null)
            _verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
        
        _spacing = _verticalLayoutGroup.spacing;

        if (sectionAnchorPrefab is null || rebindItemPrefab is null || spacerPrefab is null)
            return;

        _sectionHeight = sectionAnchorPrefab.GetComponent<RectTransform>().sizeDelta.y;
        
        _scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        
        _content = scrollRect.content;

        if (headerContainer is null)
            return;

        headerContainer.drivenByObject = this;
        headerContainer.drivenProperties = DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax;
        headerContainer.anchorMin = new Vector2(0, 1);
        headerContainer.anchorMax = Vector2.one;
        
        scrollRect.onValueChanged.AddListener(OnScroll);
        
        OnScroll(Vector2.zero);
    }

    private void Start()
    {
        OnScroll(Vector2.zero);
    }

    private void OnEnable()
    {
        OnScroll(Vector2.zero);
    }

    public void JumpTo(int sectionIndex)
    {
        if (_content is null || scrollRect is null || _scrollRectTransform is null)
            return;
        
        int sectionCount = _anchors.Count;
        if (sectionIndex >= sectionCount || sectionIndex < 0)
            return;
        
        Canvas.ForceUpdateCanvases();
        scrollRect.StopMovement();

        if (sectionIndex == 0)
        {
            scrollRect.verticalNormalizedPosition = 1;
        }
        else
        {
            var targetAnchor = _anchors[sectionIndex];
            Vector2 targetPos = _scrollRectTransform.InverseTransformPoint(_content.position) -
                                _scrollRectTransform.InverseTransformPoint(targetAnchor.RectTransform.position);
            var targetYPos = targetPos.y + (_sectionHeight / 2f) - _spacing;
        
            _content.SetAnchoredPosY(targetYPos);
        }

        if (_currentSection != sectionIndex)
            OnSectionChanged.Invoke(sectionIndex);

        _currentSection = sectionIndex;
    }

    public void AddSection(string sectionName)
    {
        if (!isActiveAndEnabled)
            return;
        
        if (sectionHeaderPrefab is null || sectionAnchorPrefab is null || scrollRect is null)
            return;

        var anchorObject = Instantiate(sectionAnchorPrefab, _content);
        var sectionAnchor = anchorObject.GetComponent<SectionHeaderAnchor>();

        var sectionObject = Instantiate(sectionHeaderPrefab, headerContainer);
        var sectionHeader = sectionObject.GetComponent<SectionHeader>();

        var sectionTransform = sectionHeader.RectTransform;
        sectionTransform.drivenByObject = this;
        sectionTransform.drivenProperties = DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMaxX |
                                            DrivenTransformProperties.AnchoredPosition;
        sectionTransform.anchorMin = new Vector2(0, sectionTransform.anchorMin.y);
        sectionTransform.anchorMax = new Vector2(1, sectionTransform.anchorMax.y);

        sectionHeader.anchor = sectionAnchor;
        sectionAnchor.sectionHeader = sectionHeader;
        
        sectionHeader.SetText(sectionName);
        
        OnScroll(Vector2.zero);

        if (_anchors.Count == 0)
            sectionAnchor.RectTransform.sizeDelta = new Vector2();
        
        _currentSection = _anchors.Count;
        
        _anchors.Add(sectionAnchor);
    }

    public void AddBinds(RemappableKey? kbmKey, RemappableKey? gamepadKey, bool isBaseGame = false, string controlName = "")
    {
        if (!isActiveAndEnabled)
            return;
        
        if (rebindItemPrefab is null || scrollRect is null)
            return;
        
        if (kbmKey is not null && string.IsNullOrEmpty(controlName))
        {
            controlName = kbmKey.ControlName;
        }
        else if (gamepadKey is not null && string.IsNullOrEmpty(controlName))
        {
            controlName = gamepadKey.ControlName;
        }

        var rebindObject = Instantiate(rebindItemPrefab, _content);
        var rebindItem = rebindObject.GetComponent<RebindItem>();
        rebindItem.SetBind(controlName, kbmKey, gamepadKey, isBaseGame);
    }

    public void AddFooter()
    {
        if (!isActiveAndEnabled)
            return;

        if (spacerPrefab is null)
            return;

        Instantiate(spacerPrefab, _content);
    }
    
    private void OnScroll(Vector2 delta)
    {
        if (_scrollRectTransform is null || headerContainer is null || _rectTransform is null)
            return;

        var maxVisibleY = headerContainer.WorldCornersMaxY();
        var topSlotPos = maxVisibleY - headerContainer.transform.position.y;

        var section = -1;
        
        for (var i = 0; i < _anchors.Count; i++)
        {
            var anchor = _anchors[i];
            var header = anchor.sectionHeader!;
            
            if (i == 0)
            {
                
                header.RectTransform.SetLocalPosY(topSlotPos - ((header.RectTransform.sizeDelta.y / 2f) - _spacing));
                section = i;
                continue;
            }
            
            var prevAnchor = _anchors[i - 1];
            var prevHeader = prevAnchor.sectionHeader!;
            
            var nextYPos = CalculateHeaderRawYPos(anchor);
            header.RectTransform.SetLocalPosY(nextYPos);

            var headerMaxY = header.RectTransform.WorldCornersMaxY();
            var prevHeaderMinY = prevHeader.RectTransform.WorldCornersMinY();
            
            var prevYPos = prevHeader.RectTransform.anchoredPosition.y;
            
            if (headerMaxY + _spacing >= prevHeaderMinY)
                prevHeader.RectTransform.SetLocalPosY(Mathf.Min(prevYPos, nextYPos + header.RectTransform.sizeDelta.y) + _spacing);

            if (headerMaxY + _spacing / 2f >= maxVisibleY)
            {
                section = i;
                header.RectTransform.SetLocalPosY(topSlotPos - ((header.RectTransform.sizeDelta.y / 2f) - _spacing));
            }
        }

        if (_currentSection != section)
            OnSectionChanged.Invoke(section);

        _currentSection = section;
    }

    private float CalculateHeaderRawYPos(SectionHeaderAnchor anchor)
    {
        if (_content is null || headerContainer is null || _scrollRectTransform is null)
            return 0;

        var offset = headerContainer.WorldCornersMaxY() - _scrollRectTransform.WorldCornersMaxY();
        
        var yPos = (anchor.RectTransform.anchoredPosition.y - (offset + 50)) + _content.anchoredPosition.y;

        return yPos;
    }

    // private void OnDrawGizmos()
    // {
    //     if (_rectTransform is null || headerContainer is null)
    //         return;
    //     
    //     var corners = new Vector3[4];
    //     var prevColor = Gizmos.color;
    //
    //     Vector3 vecSpacing = new Vector3(0, spacing, 0);
    //     
    //     headerContainer!.GetWorldCorners(corners);
    //     
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(corners[1], corners[2]);
    //     Gizmos.color = Color.white;
    //     Gizmos.DrawLine(corners[3], corners[0]);
    //     
    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawLine(corners[1] - vecSpacing, corners[2] - vecSpacing);
    //     Gizmos.DrawLine(corners[1] + vecSpacing, corners[2] + vecSpacing);
    //     
    //     foreach (var anchor in _anchors)
    //     {
    //         anchor.RectTransform.GetWorldCorners(corners);
    //         
    //         Gizmos.color = Color.yellow;
    //         Gizmos.DrawLine(corners[0], corners[1]);
    //         Gizmos.DrawLine(corners[1], corners[2]);
    //         Gizmos.DrawLine(corners[2], corners[3]);
    //         Gizmos.DrawLine(corners[3], corners[0]);
    //         
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawLine(corners[0] - vecSpacing, corners[1] + vecSpacing);
    //         Gizmos.DrawLine(corners[1] + vecSpacing, corners[2] + vecSpacing);
    //         Gizmos.DrawLine(corners[2] + vecSpacing, corners[3] - vecSpacing);
    //         Gizmos.DrawLine(corners[3] - vecSpacing, corners[0] - vecSpacing);
    //         
    //         var header = anchor.sectionHeader!;
    //         header.RectTransform.GetWorldCorners(corners);
    //         
    //         Gizmos.color = Color.cyan;
    //         Gizmos.DrawLine(corners[0], corners[1]);
    //         Gizmos.DrawLine(corners[1], corners[2]);
    //         Gizmos.DrawLine(corners[2], corners[3]);
    //         Gizmos.DrawLine(corners[3], corners[0]);
    //         
    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawLine(corners[0] - vecSpacing, corners[1] + vecSpacing);
    //         Gizmos.DrawLine(corners[1] + vecSpacing, corners[2] + vecSpacing);
    //         Gizmos.DrawLine(corners[2] + vecSpacing, corners[3] - vecSpacing);
    //         Gizmos.DrawLine(corners[3] - vecSpacing, corners[0] - vecSpacing);
    //     }
    //
    //     Gizmos.color = prevColor;
    // }
}