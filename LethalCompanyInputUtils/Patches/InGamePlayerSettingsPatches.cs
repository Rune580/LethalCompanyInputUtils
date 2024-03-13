using HarmonyLib;

namespace LethalCompanyInputUtils.Patches;

public static class InGamePlayerSettingsPatches
{
    [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SaveChangedSettings))]
    public static class SaveChangedSettingsPatch
    {
        public static void Prefix()
        {
            LcInputActionApi.SaveOverrides();
        }
    }
    
    [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.DiscardChangedSettings))]
    public static class DiscardChangedSettingsPatch
    {
        public static void Prefix()
        {
            LcInputActionApi.DiscardOverrides();
        }
    }
}