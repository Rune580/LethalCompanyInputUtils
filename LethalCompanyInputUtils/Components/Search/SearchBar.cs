using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace LethalCompanyInputUtils.Components.Search;

public class SearchBar : MonoBehaviour
{
    public TMP_InputField? searchInputField;

    public readonly UnityEvent<string> onValueChanged = new();

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

    private void OnDisable()
    {
        Clear();
    }

    private void OnInputFieldValueChanged(string text)
    {
        onValueChanged.Invoke(text);
    }
}