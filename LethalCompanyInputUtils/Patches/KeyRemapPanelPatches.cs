using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Patches;

public static class KeyRemapPanelPatches
{
    [HarmonyPatch(typeof(KepRemapPanel), "OnEnable")]
    public static class LoadKeybindsUIPatch
    {
        // ReSharper disable once InconsistentNaming
        public static bool Prefix(KepRemapPanel __instance)
        {
            LcInputActionApi.DisableForRebind();

            if (LcInputActionApi.PrefabLoaded)
            {
                __instance.remappableKeys.DisableKeys();
                return false;
            }
            
            var container =  Object.Instantiate(Assets.Load<GameObject>("Prefabs/InputUtilsRemapContainer.prefab"), __instance.transform);
            if (container is null)
                return true;
            
            var controller = container.GetComponent<RemapContainerController>();
            controller.baseGameKeys = __instance.remappableKeys;
            controller.baseGameKeys.DisableKeys();
            controller.LoadUi();
            
            LcInputActionApi.PrefabLoaded = true;
            
            __instance.transform.Find("Scroll View").gameObject.SetActive(false);
            return false;
        }
    }
    
    [HarmonyPatch(typeof(KepRemapPanel), "OnDisable")]
    public static class UnloadKeybindsUIPatch
    {
        // ReSharper disable once InconsistentNaming
        public static bool Prefix(KepRemapPanel __instance)
        {
            __instance.remappableKeys.EnableKeys();

            return false;
        }
    }
}