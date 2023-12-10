using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils;

public static class LcInputActionApi
{
    private static readonly Dictionary<string, LcInputActions> InputActionsMap = new();
    
    private static IReadOnlyCollection<LcInputActions> InputActions => InputActionsMap.Values;
    
    internal static void LoadIntoUI(KepRemapPanel panel)
    {
        var keys = panel.remappableKeys;
        var kbmKeyCount = keys.Count(key => !key.gamepadOnly);
        
        foreach (var lcInputActions in InputActions)
        {
            if (lcInputActions.Loaded)
                continue;
            
            foreach (var actionRef in lcInputActions.ActionRefs)
            {
                var name = actionRef.action.bindings.First().name;
                var kbmKey = new RemappableKey
                {
                    ControlName = name,
                    currentInput = actionRef,
                    gamepadOnly = false
                };
                keys.Insert(kbmKeyCount++, kbmKey);
                
                var gamepadKey = new RemappableKey
                {
                    ControlName = name,
                    currentInput = actionRef,
                    rebindingIndex = 1,
                    gamepadOnly = true
                };
                keys.Add(gamepadKey);
            }
            
            lcInputActions.Loaded = true;
        }

        var widthPerItem = panel.horizontalOffset;
        var maxVisibleWidth = panel.keyRemapContainer.parent.GetComponent<RectTransform>().rect.width;
        var maxItemsInRow = Mathf.Floor(maxVisibleWidth / widthPerItem);

        panel.maxVertical = kbmKeyCount / maxItemsInRow;
    }

    internal static void CalculateVerticalMaxForGamepad(KepRemapPanel panel)
    {
        var gamepadKeyCount = panel.remappableKeys.Count(key => key.gamepadOnly);
        
        var widthPerItem = panel.horizontalOffset;
        var maxVisibleWidth = panel.keyRemapContainer.parent.GetComponent<RectTransform>().rect.width;
        var maxItemsInRow = Mathf.Floor(maxVisibleWidth / widthPerItem);

        panel.maxVertical = gamepadKeyCount / maxItemsInRow;
    }

    internal static void ResetLoadedInputActions()
    {
        foreach (var lcInputActions in InputActions)
            lcInputActions.Loaded = false;
    }

    internal static void RegisterInputActions(LcInputActions lcInputActions)
    {
        if (InputActionsMap.TryAdd(lcInputActions.Id, lcInputActions))
            return;
        
        Logging.Logger.LogWarning($"The mod [{lcInputActions.Plugin.GUID}] instantiated an Actions class [{lcInputActions.GetType().Name}] more than once!\n" +
                                  $"\t These classes should be treated as singletons!, do not instantiate more than once!");
    }

    internal static void DisableForRebind()
    {
        foreach (var lcInputActions in InputActions)
        {
            if (lcInputActions.Enabled)
                lcInputActions.Disable();
        }
    }

    internal static void ReEnableFromRebind()
    {
        foreach (var lcInputActions in InputActions)
        {
            if (lcInputActions.WasEnabled)
                lcInputActions.Enable();
        }
    }
}