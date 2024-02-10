using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Data;
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

    public int ActiveSectionCount => _sectionEntries.Sum(section => section.isActiveAndEnabled ? 1 : 0);

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
        sectionEntry.sectionIndex = ActiveSectionCount;
        sectionEntry.OnEntrySelected.AddListener(OnSectionEntryPressed);
        
        _sectionEntries.Add(sectionEntry);
        KeyBindSearchManager.Instance.AddSection(sectionName, sectionEntry);
    }

    public void SelectSection(int sectionIndex)
    {
        int sectionCount = ActiveSectionCount;
        if (sectionIndex >= sectionCount || sectionIndex < 0)
            return;

        foreach (var entry in _sectionEntries)
            entry.SetIndicator(false);
        
        var sectionEntry = GetSectionEntry(sectionIndex);
        if (sectionEntry is null)
            return;
        
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
        var minEntryY = entryTransform.WorldMinY();
        var maxEntryY = entryTransform.WorldMaxY();
        
        var minVisibleY = _viewport.WorldMinY();
        var maxVisibleY = _viewport.WorldMaxY();

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

    private SectionEntry? GetSectionEntry(int index)
    {
        return _sectionEntries.Where(entry => entry.isActiveAndEnabled)
            .ElementAtOrDefault(index);
    }
}