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

            if (LcInputActionApi.PrefabLoaded)
            {
                __instance.remappableKeys.DisableKeys();
                LcInputActionApi.LayersDeep = 1;
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
            
            var controller = container.GetComponent<RemapContainerController>();
            controller.baseGameKeys = keys;
            controller.backButton = backButton;
            controller.legacyHolder = legacyHolder;
            controller.baseGameKeys.DisableKeys();
            controller.LoadUi();
            
            LcInputActionApi.PrefabLoaded = true;
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
        public static bool Prefix(KepRemapPanel __instance)
        {
            __instance.remappableKeys.EnableKeys();
            LcInputActionApi.LayersDeep = 0;
            return false;
        }
    }
}