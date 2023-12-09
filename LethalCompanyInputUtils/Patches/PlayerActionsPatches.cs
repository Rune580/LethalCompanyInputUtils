using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Patches;

public static class PlayerActionsPatches
{
    [HarmonyPatch]
    public class CtorPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(PlayerActions).GetConstructor(Array.Empty<Type>())!;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher
                .MatchForward(false,
                    new CodeMatch(code =>
                        code.Calls(typeof(InputActionAsset).GetMethod(nameof(InputActionAsset.FromJson)))),
                    new CodeMatch(code => code.opcode == OpCodes.Stfld))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(LcInputActionApi),
                        nameof(LcInputActionApi.Init), new[] { typeof(InputActionAsset) })));

            return matcher.InstructionEnumeration();
        }
    }
}