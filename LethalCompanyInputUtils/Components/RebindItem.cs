using TMPro;
using UnityEngine;

namespace LethalCompanyInputUtils.Components;

public class RebindItem : MonoBehaviour
{
    public TextMeshProUGUI? controlNameLabel;
    public RebindButton? kbmButton;
    public RebindButton? gamepadButton;

    public void SetBind(string controlName, RemappableKey? kbmKey, RemappableKey? gamepadKey, bool isBaseGame = false)
    {
        if (controlNameLabel is null)
            return;
        
        controlNameLabel.SetText(controlName);

        if (kbmButton is not null)
            kbmButton.SetKey(kbmKey, isBaseGame);

        if (gamepadButton is not null)
            gamepadButton.SetKey(gamepadKey, isBaseGame);
    }
}