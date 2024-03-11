using System;
using System.Collections;
using System.Collections.Generic;
using LethalCompanyInputUtils.Components.Section;
using UnityEngine;

namespace LethalCompanyInputUtils.Lib.Search;

internal class KeyBindSearchManager
{
    private static KeyBindSearchManager? _instance;

    public static KeyBindSearchManager Instance
    {
        get
        {
            _instance ??= new KeyBindSearchManager();
            return _instance;
        }
    }
    
    private readonly Dictionary<string, SectionEntry?> _sectionGameObjectLut = new();
    private readonly Dictionary<string, SectionHeaderAnchor?> _anchorGameObjectLut = new();
    private readonly Dictionary<string, Dictionary<string, GameObject?>> _sectionBindGameObjectLut = new();

    private KeyBindSearchManager() { }

    public void AddSection(string sectionName, SectionEntry entry)
    {
        _sectionGameObjectLut[sectionName] = entry;
    }

    public void AddAnchor(string sectionName, SectionHeaderAnchor anchor)
    {
        _anchorGameObjectLut[sectionName] = anchor;
    }

    public void AddBind(string sectionName, string controlName, GameObject bind)
    {
        if (!_sectionBindGameObjectLut.TryGetValue(sectionName, out var bindGameObjectLut))
        {
            bindGameObjectLut = new Dictionary<string, GameObject?>();
            _sectionBindGameObjectLut[sectionName] = bindGameObjectLut;
        }

        bindGameObjectLut[controlName] = bind;
    }

    public IEnumerator FilterWithSearch(string searchText)
    {
        var results = 0;
        foreach (var (sectionName, bindGameObjectLut) in _sectionBindGameObjectLut)
        {
            var keepSection = false;
            
            foreach (var (controlName, _) in bindGameObjectLut)
            {
                var bindActive = MatchesFilter(controlName, searchText);
                if (bindActive)
                {
                    keepSection = true;
                    results++;
                }
                
                SetBindActive(sectionName, controlName, bindActive);
            }

            SetSectionObjectsActive(sectionName, keepSection);
            
            yield return null;
        }

        yield return results;
    }

    public void Clear()
    {
        _sectionGameObjectLut.Clear();
        _anchorGameObjectLut.Clear();
        _sectionBindGameObjectLut.Clear();
    }

    private void SetSectionActive(string sectionName, bool active)
    {
        if (!_sectionBindGameObjectLut.TryGetValue(sectionName, out var bindGameObjectLut))
            return;

        foreach (var bind in bindGameObjectLut.Values)
        {
            if (bind is null)
                continue;
            
            bind.SetActive(active);
        }

        SetSectionObjectsActive(sectionName, active);
    }

    private void SetSectionObjectsActive(string sectionName, bool active)
    {
        if (_sectionGameObjectLut.TryGetValue(sectionName, out var entry))
        {
            if (entry is not null)
            {
                entry.gameObject.SetActive(active);
                
                var index = 0;
                foreach (var (key, lutEntry) in _sectionGameObjectLut)
                {
                    if (key == sectionName)
                        break;

                    if (lutEntry is null)
                        continue;

                    if (!lutEntry.isActiveAndEnabled)
                        continue;

                    index++;
                }

                entry.sectionIndex = index;
            }
        }

        if (_anchorGameObjectLut.TryGetValue(sectionName, out var anchor))
        {
            if (anchor is not null)
            {
                var header = anchor.sectionHeader;
                if (header is not null)
                    header.gameObject.SetActive(active);
                
                anchor.gameObject.SetActive(active);
            }
        }
    }

    private void SetBindActive(string sectionName, string controlName, bool active)
    {
        if (!_sectionBindGameObjectLut.TryGetValue(sectionName, out var bindGameObjectLut))
            return;

        if (!bindGameObjectLut.TryGetValue(controlName, out var bind))
            return;

        if (bind is null)
            return;
        
        bind.SetActive(active);
    }

    private static bool MatchesFilter(string text, string filter)
    {
        if (string.Equals(text, filter, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return text.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
    }
}