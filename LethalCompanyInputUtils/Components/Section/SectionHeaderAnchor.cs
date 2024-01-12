using System;
using UnityEngine;

namespace LethalCompanyInputUtils.Components.Section;

[RequireComponent(typeof(RectTransform))]
public class SectionHeaderAnchor : MonoBehaviour
{
    public SectionHeader? sectionHeader;
    
    private RectTransform? _rectTransform;
    
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform is null)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }

    private void Awake()
    {
        if (_rectTransform is null)
            _rectTransform = GetComponent<RectTransform>();
    }
}