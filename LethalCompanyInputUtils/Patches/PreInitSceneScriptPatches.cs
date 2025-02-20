using HarmonyLib;
using LethalCompanyInputUtils.Data;

namespace LethalCompanyInputUtils.Patches;

public static class PreInitSceneScriptPatches
{
    [HarmonyPatch(typeof(PreInitSceneScript), nameof(PreInitSceneScript.SkipToFinalSetting))]
    public static class SkipToFinalSettingPatch
    {
        public static void Postfix()
        {
            VanillaInputActions.Instance = new VanillaInputActions();
            VanillaInputActions.Instance.Load();
        }
    }
}