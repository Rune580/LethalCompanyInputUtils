using HarmonyLib;

namespace LethalCompanyInputUtils.Patches;

public static class QuickMenuManagerPatches
{
    [HarmonyPatch(typeof(QuickMenuManager), nameof(QuickMenuManager.CloseQuickMenu))]
    public static class OpenMenuPerformedPatch
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
    }
}