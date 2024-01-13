using TMPro;
using UnityEngine;

namespace LethalCompanyInputUtils.Components.Section;

[RequireComponent(typeof(RectTransform))]
public class SectionHeader : MonoBehaviour
{
    public SectionHeaderAnchor? anchor;
    public TextMeshProUGUI? label;
    
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

    public void SetText(string text)
    {
        if (label is null)
            return;
        
        label.SetText(text);
    }
}