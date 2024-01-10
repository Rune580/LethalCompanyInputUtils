using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LethalCompanyInputUtils.Components;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Patches;

public static class KeyRemapPanelPatches
{
    [HarmonyPatch(typeof(KepRemapPanel), nameof(KepRemapPanel.LoadKeybindsUI))]
    public static class LoadKeybindsUIPatch
    {
        // ReSharper disable once InconsistentNaming
        public static bool Prefix(KepRemapPanel __instance)
        {
            LcInputActionApi.DisableForRebind();
            // LcInputActionApi.LoadIntoUI(__instance);

            if (LcInputActionApi.PrefabLoaded)
                return false;
            
            var container =  Object.Instantiate(Assets.Load<GameObject>("Prefabs/InputUtilsRemapContainer.prefab"), __instance.transform);
            if (container is null)
                return true;
            
            var controller = container.GetComponent<RemapContainerController>();
            controller.baseGameKeys = __instance.remappableKeys;
            controller.LoadUi();
            
            LcInputActionApi.PrefabLoaded = true;
            
            __instance.transform.Find("Scroll View").gameObject.SetActive(false);
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            var maxVerticalField = AccessTools.Field(typeof(KepRemapPanel), nameof(KepRemapPanel.maxVertical));

            matcher
                .MatchForward(true,
                    new CodeMatch(code => code.IsLdarg(0)),
                    new CodeMatch(code =>
                        code.LoadsField(maxVerticalField)),
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    new CodeMatch(code => code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 2f),
                    new CodeMatch(code => code.opcode == OpCodes.Add),
                    new CodeMatch(code => code.opcode == OpCodes.Conv_I4),
                    new CodeMatch(code => code.IsStloc())
                    );
                
            matcher
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(LcInputActionApi),
                        nameof(LcInputActionApi.CalculateVerticalMaxForGamepad), new[] { typeof(KepRemapPanel) })));

            return matcher.InstructionEnumeration();
        }
    }
    
    [HarmonyPatch(typeof(KepRemapPanel), nameof(KepRemapPanel.UnloadKeybindsUI))]
    public static class UnloadKeybindsUIPatch
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix()
        {
            LcInputActionApi.SaveOverrides();
            LcInputActionApi.ReEnableFromRebind();
        }
    }
}