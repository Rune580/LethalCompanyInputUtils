using System;
using HarmonyLib;

namespace LethalCompanyInputUtils.Patches;

public static class QuickMenuManagerPatches
{
    public static event Action? OnExitMenuRequested;
    
    [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.CloseQuickMenu))]
    public static class OpenMenuPerformedPatch
    {
        // ReSharper disable once InconsistentNaming
        public static bool Prefix(QuickMenuManager __instance)
        {
            if (LcInputActionApi.RemapContainerVisible() && __instance.isMenuOpen)
            {
                LcInputActionApi.CloseContainerLayer();
                OnExitMenuRequested?.Invoke();
                
                return false;
            }

            return true;
        }
    }
}