using System.Collections.Generic;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components.Section;

public class SectionListController : MonoBehaviour
{
    public GameObject? sectionEntryPrefab;
    public ScrollRect? scrollRect;
    public RemapContainerController? remapContainer;
    
    private RectTransform? _viewport;
    private RectTransform? _content;
    
    private readonly List<SectionEntry> _sectionEntries = [];

    private void Awake()
    {
        if (remapContainer is null)
            remapContainer = GetComponentInParent<RemapContainerController>();

        if (scrollRect is null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        _viewport = scrollRect.viewport;
        _content = scrollRect.content;
    }

    public void AddSection(string sectionName)
    {
        if (_content is null || sectionEntryPrefab is null)
            return;

        var sectionObject = Instantiate(sectionEntryPrefab, _content);
        var sectionEntry = sectionObject.GetComponent<SectionEntry>();
        
        sectionEntry.SetText(sectionName);
        sectionEntry.sectionIndex = _sectionEntries.Count;
        sectionEntry.OnEntrySelected.AddListener(OnSectionEntryPressed);
        
        _sectionEntries.Add(sectionEntry);
    }

    public void SelectSection(int sectionIndex)
    {
        int sectionCount = _sectionEntries.Count;
        if (sectionIndex >= sectionCount || sectionIndex < 0)
            return;

        foreach (var entry in _sectionEntries)
            entry.SetIndicator(false);
        
        var sectionEntry = _sectionEntries[sectionIndex];
        sectionEntry.SetIndicator(true);

        if (scrollRect is null)
            return;

        UpdateScrollPosToFit(sectionEntry);
    }

    private void UpdateScrollPosToFit(SectionEntry sectionEntry)
    {
        if (_viewport is null || _content is null || scrollRect is null)
            return;
        
        var entryTransform = sectionEntry.RectTransform;
        var minEntryY = entryTransform.WorldCornersMinY();
        var maxEntryY = entryTransform.WorldCornersMaxY();
        
        var minVisibleY = _viewport.WorldCornersMinY();
        var maxVisibleY = _viewport.WorldCornersMaxY();

        if (minEntryY > minVisibleY && maxEntryY < maxVisibleY)
            return;
        
        scrollRect.StopMovement();

        float dist = 0;
        if (maxEntryY > maxVisibleY)
            dist = maxVisibleY - maxEntryY - entryTransform.sizeDelta.y;
        else if (minEntryY < minVisibleY)
            dist = (minVisibleY - minEntryY) + entryTransform.sizeDelta.y;

        var currentY = _content.anchoredPosition.y;
        _content.SetAnchoredPosY(currentY + dist);
    }

    private void OnSectionEntryPressed(int sectionIndex)
    {
        if (remapContainer is null)
            return;
        
        remapContainer.JumpTo(sectionIndex);
    }
}