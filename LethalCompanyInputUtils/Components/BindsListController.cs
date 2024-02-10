using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Components.Section;
using LethalCompanyInputUtils.Data;
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

    public static float OffsetCompensation = 0;

    private RectTransform? _rectTransform;
    private RectTransform? _scrollRectTransform;
    private RectTransform? _content;
    private VerticalLayoutGroup? _verticalLayoutGroup;

    private int _currentSection;
    private float _sectionHeight;
    private float _spacing;

    private readonly List<SectionHeaderAnchor> _anchors = [];

    public int ActiveAnchorCount => _anchors.Sum(anchor => anchor.isActiveAndEnabled ? 1 : 0);

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
        
        int sectionCount = ActiveAnchorCount;
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
            var targetAnchor = GetAnchor(sectionIndex);
            if (targetAnchor is null)
                return;
            
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

        var anchorCount = ActiveAnchorCount;
        if (anchorCount == 0)
            sectionAnchor.RectTransform.sizeDelta = new Vector2();
        
        _currentSection = anchorCount;
        
        _anchors.Add(sectionAnchor);
        KeyBindSearchManager.Instance.AddAnchor(sectionName, sectionAnchor);
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

        var sectionName = GetCurrentSectionName();
        if (string.IsNullOrWhiteSpace(sectionName))
            return;
        
        KeyBindSearchManager.Instance.AddBind(sectionName, controlName, rebindObject);
    }

    public void AddFooter()
    {
        if (!isActiveAndEnabled)
            return;

        if (spacerPrefab is null)
            return;

        Instantiate(spacerPrefab, _content);
    }
    
    // This function is such a mess right now, I promise to clean it up when I have more time.
    private void OnScroll(Vector2 delta)
    {
        if (_scrollRectTransform is null || headerContainer is null || _rectTransform is null)
            return;
        
        var maxVisibleY = GetMaxY(headerContainer);

        var section = -1;
        
        for (var i = 0; i < ActiveAnchorCount; i++)
        {
            var anchor = GetAnchor(i);
            if (anchor is null)
                continue;
            
            var header = anchor.sectionHeader!;
            
            if (i == 0)
            {
                header.RectTransform.SetLocalPosY(maxVisibleY - ((_sectionHeight / 2f) - _spacing));
                section = i;
                continue;
            }
            
            var prevAnchor = GetAnchor(i - 1);
            if (prevAnchor is null)
                continue;
            
            var prevHeader = prevAnchor.sectionHeader!;
            
            var nextYPos = CalculateHeaderRawYPos(anchor);
            header.RectTransform.SetLocalPosY(nextYPos);

            var headerMaxY = GetMaxY(header.RectTransform) + header.RectTransform.localPosition.y;
            var prevHeaderMinY = GetMinY(prevHeader.RectTransform) + prevHeader.RectTransform.localPosition.y;
            
            if (headerMaxY + (_sectionHeight / 2f) + _spacing >= prevHeaderMinY)
                prevHeader.RectTransform.SetLocalPosY(nextYPos + _sectionHeight);

            if (headerMaxY + _spacing / 2f >= maxVisibleY - _sectionHeight / 2f)
            {
                section = i;
                header.RectTransform.SetLocalPosY(maxVisibleY - ((_sectionHeight / 2f) - _spacing));
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

        var offset = GetMaxY(headerContainer) - GetMaxY(_scrollRectTransform);
        offset += _sectionHeight / 2f;
        offset -= OffsetCompensation;
        
        var yPos = (anchor.RectTransform.localPosition.y - (offset + 50)) + _content.localPosition.y;
        
        return yPos;
    }

    private void OnDrawGizmos()
    {
        if (_rectTransform is null || headerContainer is null)
            return;
        
        var prevColor = Gizmos.color;
    
        _rectTransform.DrawGizmoUiRectWorld();
    
        Gizmos.color = prevColor;
    }

    private string? GetCurrentSectionName()
    {
        if (ActiveAnchorCount == 0)
            return null;

        var anchor = GetAnchor(_currentSection);
        if (anchor is null)
            return null;

        var header = anchor.sectionHeader;
        if (header is null)
            return null;

        var label = header.label;
        if (label is null)
            return null;

        return label.text;
    }

    private SectionHeaderAnchor? GetAnchor(int index)
    {
        return _anchors.Where(anchor => anchor.isActiveAndEnabled)
            .ElementAtOrDefault(index);
    }

    private float GetMaxY(RectTransform element)
    {
        if (_rectTransform is null)
            _rectTransform = GetComponent<RectTransform>();

        return element.UiBounds(Vector3.zero).max.y;
    }

    private float GetMinY(RectTransform element)
    {
        if (_rectTransform is null)
            _rectTransform = GetComponent<RectTransform>();
        
        return element.UiBounds(Vector3.zero).min.y;
    }
}