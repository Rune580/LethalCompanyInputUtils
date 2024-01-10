using System.Collections.Generic;
using UnityEngine;

namespace LethalCompanyInputUtils.Components;

public class RemapContainerController : MonoBehaviour
{
    public GameObject? sectionHeaderPrefab;
    public GameObject? rebindItemPrefab;

    public RectTransform? bindsContentTransform;

    public List<RemappableKey> baseGameKeys = [];

    public void LoadUi()
    {
        // <(Keyboard/Mouse Key, Gamepad Key)>
        var pairedKeys = new Dictionary<string, (RemappableKey?, RemappableKey?)>();
        foreach (var baseGameKey in baseGameKeys)
        {
            RemappableKey? kbmKey = null;
            RemappableKey? gamepadKey = null;
            
            if (pairedKeys.TryGetValue(baseGameKey.ControlName, out var keyPair))
            {
                if (baseGameKey.gamepadOnly)
                {
                    kbmKey = keyPair.Item1;
                }
                else
                {
                    gamepadKey = keyPair.Item2;
                }
            }

            if (baseGameKey.gamepadOnly)
            {
                gamepadKey = baseGameKey;
            }
            else
            {
                kbmKey = baseGameKey;
            }

            pairedKeys[baseGameKey.ControlName] = (kbmKey, gamepadKey);
        }
        
        GenerateBaseGameSection(pairedKeys);
    }

    private void GenerateBaseGameSection(Dictionary<string, (RemappableKey?, RemappableKey?)> pairedKeys)
    {
        NewSection("Lethal Company");
        foreach (var (_, (kbmKey, gamepadKey)) in pairedKeys)
            NewBind(kbmKey, gamepadKey, true);
    }

    private void NewSection(string sectionName)
    {
        if (sectionHeaderPrefab is null || bindsContentTransform is null)
            return;

        var sectionObject = Instantiate(sectionHeaderPrefab, bindsContentTransform);
        var sectionHeader = sectionObject.GetComponent<SectionHeader>();
        sectionHeader.SetText(sectionName);
    }

    private void NewBind(RemappableKey? kbmKey, RemappableKey? gamepadKey, bool isBaseGame = false)
    {
        if (rebindItemPrefab is null || bindsContentTransform is null)
            return;
        
        string controlName = "";
        if (kbmKey is not null)
        {
            controlName = kbmKey.ControlName;
        }
        else if (gamepadKey is not null)
        {
            controlName = gamepadKey.ControlName;
        }

        var rebindObject = Instantiate(rebindItemPrefab, bindsContentTransform);
        var rebindItem = rebindObject.GetComponent<RebindItem>();
        rebindItem.SetBind(controlName, kbmKey, gamepadKey, isBaseGame);
    }
}