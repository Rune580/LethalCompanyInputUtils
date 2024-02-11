using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace LethalCompanyInputUtils.Components.Search;

public class SearchBar : MonoBehaviour
{
    public TMP_InputField? searchInputField;
    public Color normalColor;
    public Color noResultsColor;

    public readonly UnityEvent<string> onValueChanged = new();
    
    private bool _hasResults;

    private void Awake()
    {
        if (searchInputField is null)
            return;
        
        searchInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    public void Clear()
    {
        if (searchInputField is null)
            return;

        searchInputField.text = "";
    }

    public void WithResults(int count)
    {
        _hasResults = count > 0;
        
        UpdateState();
    }

    private void UpdateState()
    {
        if (searchInputField is null)
            return;

        searchInputField.textComponent.color = _hasResults ? normalColor : noResultsColor;
    }

    private void OnDisable()
    {
        Clear();
    }

    private void OnInputFieldValueChanged(string text)
    {
        onValueChanged.Invoke(text);
    }
}