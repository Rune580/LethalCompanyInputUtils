using HarmonyLib;
using LethalCompanyInputUtils.Data;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Patches;

public static class QuickMenuManagerPatches
{
    [HarmonyPatch(typeof(QuickMenuManager), "Start")]
    public static class StartPatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix(QuickMenuManager __instance)
        {
            Object.Instantiate(Assets.Load<GameObject>("Prefabs/PopOver Layer.prefab"), __instance.menuContainer.transform);
        }
    }
    
    [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.CloseQuickMenu))]
    public static class CloseQuickMenuPatch
    {
        // ReSharper disable once InconsistentNaming
        public static bool Prefix(QuickMenuManager __instance)
        {
            if (LcInputActionApi.RemapContainerVisible() && __instance.isMenuOpen)
            {
                LcInputActionApi.CloseContainerLayer();
                return false;
            }

            return true;
        }

        public static void Postfix()
        {
            VanillaInputActions.Instance.Load();
        }
    }
}