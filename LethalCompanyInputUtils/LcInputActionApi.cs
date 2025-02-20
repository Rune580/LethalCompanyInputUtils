using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Localization;
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
    internal static RemapContainerController? ContainerInstance;
    
    internal static IReadOnlyCollection<LcInputActions> InputActions => InputActionsMap.Values;
    
    // Try and find any controls that were manually injected into the UI.
    internal static void LoadIntoUI(KepRemapPanel panel)
    {
        AdjustSizeAndPos(panel);
        var layoutElement = EnsureLayoutElement(panel);
        
        panel.LoadKeybindsUI();
        
        var widthPerItem = panel.horizontalOffset;
        var maxVisibleWidth = panel.keyRemapContainer.parent.GetComponent<RectTransform>().rect.width;
        var maxItemsInRow = Mathf.Floor(maxVisibleWidth / widthPerItem);
        
        var keySlotCount = panel.keySlots.Count;
        var actualKeyCount = NumberOfActualKeys(panel.keySlots);
        var sectionCount = keySlotCount - actualKeyCount;
        
        panel.maxVertical = (actualKeyCount / maxItemsInRow) + sectionCount;

        if (ContainerInstance is not null && ContainerInstance.legacyButton is not null)
        {
            var label = ContainerInstance.legacyButton.GetComponentInChildren<TextMeshProUGUI>();
            label.SetText($"> {string.Format(LocaleManager.GetString("LegacyControls.Button.Label"),  actualKeyCount)}");
        }

        if (keySlotCount == 0)
            return;

        layoutElement.minHeight = (panel.maxVertical + 1) * panel.verticalOffset;

        int row = 0;
        int column = 0;
        foreach (var keySlot in panel.keySlots)
        {
            if (column > maxItemsInRow)
            {
                row++;
                column = 0;
            }
            
            var rectTransform = keySlot.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(column * panel.horizontalOffset, row * -panel.verticalOffset);
            
            if (keySlot.GetComponentInChildren<SettingsOption>() is null) // is Section object
            {
                column = 0;
                row++;
            }
            else // is KeyBind object
            {
                column++;
            }
        }
    }

    public static bool RemapContainerVisible()
    {
        if (ContainerInstance is null)
            return false;

        return ContainerInstance.LayerShown > 0;
    }

    private static int NumberOfActualKeys(List<GameObject> keySlots)
    {
        int i = 0;
        
        foreach (var keySlot in keySlots)
        {
            if (keySlot.GetComponentInChildren<SettingsOption>() is not null)
                i++;
        }

        return i;
    }

    internal static void CloseContainerLayer()
    {
        if (ContainerInstance is null)
            return;
        
        ContainerInstance.HideHighestLayer();
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
            Logging.Warn(
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
        if (ContainerInstance is null)
            return;
        
        ContainerInstance.SaveOverrides();
    }

    internal static void DiscardOverrides()
    {
        if (ContainerInstance is null)
            return;
        
        ContainerInstance.DiscardOverrides();
    }
}