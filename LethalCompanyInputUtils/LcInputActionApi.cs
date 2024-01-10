using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LethalCompanyInputUtils;

public static class LcInputActionApi
{
    private static readonly Dictionary<string, LcInputActions> InputActionsMap = new();
    internal static bool PrefabLoaded;
    
    private static IReadOnlyCollection<LcInputActions> InputActions => InputActionsMap.Values;
    
    internal static void LoadIntoUI(KepRemapPanel panel)
    {
        UpdateFontScaling(panel);
        AdjustSizeAndPos(panel);
        var layoutElement = EnsureLayoutElement(panel);
        layoutElement.minHeight = 0;
        
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
        layoutElement.minHeight += (panel.maxVertical + 1) * panel.verticalOffset;
    }

    private static void UpdateFontScaling(KepRemapPanel panel)
    {
        var textMeshPro = panel.keyRemapSlotPrefab.transform.Find("Text (1)")
            .GetComponent<TextMeshProUGUI>();

        if (textMeshPro.enableAutoSizing)
            return;

        textMeshPro.fontSizeMax = textMeshPro.fontSize;
        textMeshPro.fontSizeMin = textMeshPro.fontSize - 4;
        textMeshPro.enableAutoSizing = true;
    }

    private static void AdjustSizeAndPos(KepRemapPanel panel)
    {
        var content = panel.keyRemapContainer.parent.gameObject;
        if (content.GetComponent<ContentSizeFitter>() is not null)
            return;

        panel.keyRemapContainer.SetPivotY(1);
        panel.keyRemapContainer.SetAnchorMinY(1);
        panel.keyRemapContainer.SetAnchorMaxY(1);
        panel.keyRemapContainer.SetAnchoredPosY(0);
        panel.keyRemapContainer.SetLocalPosY(0);

        var sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
    }

    private static LayoutElement EnsureLayoutElement(KepRemapPanel panel)
    {
        var content = panel.keyRemapContainer.parent.gameObject;

        var layoutElement = content.GetComponent<LayoutElement>();
        if (layoutElement is not null)
            return layoutElement;

        layoutElement = content.AddComponent<LayoutElement>();
        return layoutElement;
    }

    internal static void CalculateVerticalMaxForGamepad(KepRemapPanel panel)
    {
        var gamepadKeyCount = panel.remappableKeys.Count(key => key.gamepadOnly);
        
        var widthPerItem = panel.horizontalOffset;
        var maxVisibleWidth = panel.keyRemapContainer.parent.GetComponent<RectTransform>().rect.width;
        var maxItemsInRow = Mathf.Floor(maxVisibleWidth / widthPerItem);

        panel.maxVertical = gamepadKeyCount / maxItemsInRow;

        var layoutElement = EnsureLayoutElement(panel);
        layoutElement.minHeight += (panel.maxVertical + 3) * panel.verticalOffset;
    }

    internal static void ResetLoadedInputActions()
    {
        PrefabLoaded = false;
        
        foreach (var lcInputActions in InputActions)
            lcInputActions.Loaded = false;
    }

    internal static void RegisterInputActions(LcInputActions lcInputActions, InputActionMapBuilder builder)
    {
        if (!InputActionsMap.TryAdd(lcInputActions.Id, lcInputActions))
        {
            Logging.Logger.LogWarning(
                $"The mod [{lcInputActions.Plugin.GUID}] instantiated an Actions class [{lcInputActions.GetType().Name}] more than once!\n" +
                $"\t These classes should be treated as singletons!, do not instantiate more than once!");
            
            return;
        }
        
        lcInputActions.CreateInputActions(builder);
        
        lcInputActions.GetAsset()
            .AddActionMap(builder.Build());
        lcInputActions.GetAsset()
            .Enable();
        
        lcInputActions.OnAssetLoaded();
        lcInputActions.Load();
        
        lcInputActions.BuildActionRefs();
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

    internal static void SaveOverrides()
    {
        foreach (var lcInputAction in InputActions)
            lcInputAction.Save();
    }
}