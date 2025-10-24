using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Patches;

public static class InputManagerPatches
{
    [HarmonyPatch(typeof(InputManager), nameof(InputManager.RegisterCustomTypes), [])]
    public static class RegisterCustomTypesPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!InputUtilsConfig.patchInputManagerUseSaferRegisterOfCustomTypes.Value)
                return instructions;
            
            var matcher = new CodeMatcher(instructions);

            var registerTypesMethodInfo = AccessTools.Method(typeof(InputManager), nameof(InputManager.RegisterCustomTypes), [typeof(Type[])]);
            var safeRegisterTypesMethodInfo = AccessTools.Method(typeof(InputManagerPatches), nameof(RegisterCustomTypesSafe));

            matcher.MatchForward(true,
                new CodeMatch(code =>
                    code.opcode == OpCodes.Call &&
                    code.Calls(registerTypesMethodInfo))
            );

            if (matcher.IsInvalid)
            {
                Logging.Warn("Failed to patch InputManager.RegisterCustomTypes!");
                return matcher.InstructionEnumeration();
            }

            matcher.RemoveInstruction();
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, safeRegisterTypesMethodInfo));
            
            return matcher.InstructionEnumeration();
        }
    }

    private static void RegisterCustomTypesSafe(InputManager instance, Type[] types)
    {
        // Stupid way to do this, but I'm too lazy to figure out how to insert a try catch in IL
        foreach (var type in types)
        {
            try
            {
                instance.RegisterCustomTypes([type]);
            }
            catch 
            {
                // Catch all exceptions
            }
        }
    }
}