using HarmonyLib;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Patches;

public static class KeyRemapPanelPatches
{
    [HarmonyPatch(typeof(KepRemapPanel), "OnEnable")]
    public static class LoadKeybindsUIPatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix(KepRemapPanel __instance)
        {
            LcInputActionApi.DisableForRebind();

            if (LcInputActionApi.prefabLoaded && LcInputActionApi.containerInstance is not null)
            {
                LcInputActionApi.containerInstance.baseGameKeys.DisableKeys();
                return;
            }
            
            var container =  Object.Instantiate(Assets.Load<GameObject>("Prefabs/InputUtilsRemapContainer.prefab"), __instance.transform);
            var legacyHolder = Object.Instantiate(Assets.Load<GameObject>("Prefabs/Legacy Holder.prefab"), __instance.transform);
            if (container is null || legacyHolder is null)
                return;

            var legacySection = __instance.transform.Find("Scroll View");
            if (legacySection is null)
                return;
            
            legacySection.SetParent(legacyHolder.transform);
            var keys = __instance.remappableKeys;
            __instance.remappableKeys = [];
            
            legacyHolder.SetActive(false);

            var backButtonObject = __instance.transform.Find("Back").gameObject;
            var backButton = backButtonObject.GetComponent<Button>();

            var legacyBackButtonObject = Object.Instantiate(backButtonObject, legacyHolder.transform, true);
            Object.DestroyImmediate(legacyBackButtonObject.GetComponentInChildren<SettingsOption>());
            var legacyBackButton = legacyBackButtonObject.GetComponent<Button>();
            legacyBackButton.onClick = new Button.ButtonClickedEvent();
            legacyBackButton.onClick.AddListener(LcInputActionApi.CloseContainerLayer);

            var showLegacyButtonObject = Object.Instantiate(backButtonObject, backButtonObject.transform.parent);
            Object.DestroyImmediate(showLegacyButtonObject.GetComponentInChildren<SettingsOption>());
            var showLegacyButton = showLegacyButtonObject.GetComponent<Button>();
            showLegacyButton.onClick = new Button.ButtonClickedEvent();
            
            var legacyButtonTransform = showLegacyButtonObject.GetComponent<RectTransform>();
            legacyButtonTransform.SetAnchoredPosY(legacyButtonTransform.anchoredPosition.y + 25);
            legacyButtonTransform.SetSizeDeltaX(legacyButtonTransform.sizeDelta.x + 180);

            var legacySelection = legacyButtonTransform.Find("SelectionHighlight").GetComponent<RectTransform>();
            legacySelection.SetSizeDeltaX(410);
            legacySelection.offsetMax = new Vector2(410, legacySelection.offsetMax.y);
            legacySelection.offsetMin = new Vector2(0, legacySelection.offsetMin.y);
            
            var controller = container.GetComponent<RemapContainerController>();
            controller.baseGameKeys = keys;
            controller.backButton = backButton;
            controller.legacyButton = showLegacyButton;
            controller.legacyHolder = legacyHolder;
            controller.baseGameKeys.DisableKeys();
            
            showLegacyButton.onClick.AddListener(controller.ShowLegacyUi);
            
            controller.LoadUi();

            var setDefaultObject = __instance.transform.Find("SetDefault").gameObject;
            var setDefaultButton = setDefaultObject.GetComponent<Button>();
            setDefaultButton.onClick.RemoveAllListeners();
            setDefaultButton.onClick.AddListener(controller.OnSetToDefault);
            
            legacyHolder.transform.SetAsLastSibling();
            
            LcInputActionApi.prefabLoaded = true;
        }
        
        // ReSharper disable once InconsistentNaming
        public static void Postfix(KepRemapPanel __instance)
        {
            LcInputActionApi.LoadIntoUI(__instance);
        }
    }
    
    [HarmonyPatch(typeof(KepRemapPanel), "OnDisable")]
    public static class UnloadKeybindsUIPatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix()
        {
            if (LcInputActionApi.containerInstance is null)
                return;
            
            LcInputActionApi.containerInstance.baseGameKeys.EnableKeys();
        }
    }
}