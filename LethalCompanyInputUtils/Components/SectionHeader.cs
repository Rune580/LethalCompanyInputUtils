using TMPro;
using UnityEngine;

namespace LethalCompanyInputUtils.Components;

public class SectionHeader : MonoBehaviour
{
    public TextMeshProUGUI? label;

    public void SetText(string text)
    {
        if (label is null)
            return;
        
        label.SetText(text);
    }
}