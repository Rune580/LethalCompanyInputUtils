using LethalCompanyInputUtils.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LethalCompanyInputUtils.Components.PopOvers;

public class PopOverTextContainer : UIBehaviour
{
    public TextMeshProUGUI? label;
    public RectTransform? rectTransform;

    public void SetText(string text)
    {
        if (label is null)
            return;
        
        label.SetText(text);
    }
}