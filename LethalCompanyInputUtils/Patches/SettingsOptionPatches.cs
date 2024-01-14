using HarmonyLib;
using LethalCompanyInputUtils.Api;

namespace LethalCompanyInputUtils.Patches;

public static class SettingsOptionPatches
{
    /// <summary>
    /// When a base game KeyBind is "unbound" we need to patch to return early, otherwise it causes an error as unity is unable
    /// to resolve the control for the "unbound" path.
    /// </summary>
    [HarmonyPatch(typeof(SettingsOption), nameof(SettingsOption.SetBindingToCurrentSetting))]
    public static class SetBindingToCurrentSettingPatch
    {
        // ReSharper disable once InconsistentNaming
        public static bool Prefix(SettingsOption __instance)
        {
            if (__instance.optionType != SettingsOptionType.ChangeBinding)
                return true;

            var action = __instance.rebindableAction.action;
            
            foreach (var binding in action.bindings)
            {
                if (__instance.gamepadOnlyRebinding)
                {
                    if (!string.Equals(binding.effectivePath, LcInputActions.UnboundGamepadIdentifier))
                        continue;
                }
                else
                {
                    if (!string.Equals(binding.effectivePath, LcInputActions.UnboundKeyboardAndMouseIdentifier))
                        continue;
                }
                    
                __instance.currentlyUsedKeyText.SetText("");
                return false;
            }

            return true;
        }
    }
}