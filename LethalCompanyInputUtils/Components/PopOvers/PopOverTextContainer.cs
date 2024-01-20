using LethalCompanyInputUtils.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LethalCompanyInputUtils.Components.PopOvers;

public class PopOverTextContainer : UIBehaviour
{
    public TextMeshProUGUI? label;
    public RectTransform? rectTransform;

    public float Width
    {
        get
        {
            if (rectTransform is null)
                return 0;

            return rectTransform.UiBounds().width;
        }
    }

    public float Height
    {
        get
        {
            if (rectTransform is null)
                return 0;

            return rectTransform.UiBounds().height;
        }
    }

    public void SetText(string text)
    {
        if (label is null)
            return;
        
        label.SetText(text);
    }
}