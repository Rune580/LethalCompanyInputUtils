using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components.Section;

[RequireComponent(typeof(Button), typeof(RectTransform))]
public class SectionEntry : MonoBehaviour
{
    public TextMeshProUGUI? indicator;
    public TextMeshProUGUI? label;
    public Button? button;
    public int sectionIndex;
    
    public UnityEvent<int> OnEntrySelected = new();
    
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
        if (button is null)
            button = GetComponent<Button>();
        
        button.onClick.AddListener(SelectEntry);
    }

    public void SetText(string text)
    {
        if (label is null)
            return;
        
        label.SetText(text);
    }

    public void SetIndicator(bool indicated)
    {
        if (indicator is null)
            return;

        indicator.enabled = indicated;
    }
    
    private void SelectEntry()
    {
        OnEntrySelected.Invoke(sectionIndex);
    }
}