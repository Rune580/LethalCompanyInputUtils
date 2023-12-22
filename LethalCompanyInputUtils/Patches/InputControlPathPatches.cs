using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Patches;

public static class InputControlPathPatches
{
    [HarmonyPatch]
    public static class ToHumanReadableStringPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(InputControlPath))
                .Where(method => method.Name == "ToHumanReadableString" && method.ReturnType == typeof(string));
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref string __result)
        {
            if (__result is not (LcInputActions.UnboundGamepadIdentifier
                or LcInputActions.UnboundKeyboardAndMouseIdentifier))
                return;

            __result = "";
        }
    }
}