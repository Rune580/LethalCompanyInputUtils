using HarmonyLib;
using LethalCompanyInputUtils.Data;

namespace LethalCompanyInputUtils.Patches;

public static class InGamePlayerSettingsPatches
{
    [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.UpdateGameToMatchSettings))]
    public static class UpdateGameToMatchSettingsPatch
    {
        public static void Prefix()
        {
            VanillaInputActions.Instance.Load();
        }
    }
    
    [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SaveChangedSettings))]
    public static class SaveChangedSettingsPatch
    {
        public static void Prefix()
        {
            LcInputActionApi.SaveOverrides();
        }

        public static void Postfix()
        {
            VanillaInputActions.Instance.Load();
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