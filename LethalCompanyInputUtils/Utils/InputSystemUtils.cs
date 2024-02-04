using System;
using System.Linq;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Utils;

internal static class InputSystemUtils
{
    public static bool IsGamepadOnly(this InputAction action) => action.bindings.All(binding => !binding.IsKbmBind());

    public static bool IsKbmBind(this InputBinding binding) => string.Equals(binding.groups, DeviceGroups.KeyboardAndMouse);

    public static bool IsGamepadBind(this InputBinding binding) => string.Equals(binding.groups, DeviceGroups.Gamepad);

    public static RemappableKey? GetKbmKey(this InputActionReference actionRef)
    {
        var bindings = actionRef.action.bindings;
        
        for (var i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            if (!binding.IsKbmBind())
                continue;
            
            return new RemappableKey
            {
                ControlName = binding.name,
                currentInput = actionRef,
                rebindingIndex = i,
                gamepadOnly = false
            };
        }

        return null;
    }

    public static RemappableKey? GetGamepadKey(this InputActionReference actionRef)
    {
        var bindings = actionRef.action.bindings;

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            if (!binding.IsGamepadBind())
                continue;

            return new RemappableKey
            {
                ControlName = binding.name,
                currentInput = actionRef,
                rebindingIndex = i,
                gamepadOnly = true
            };
        }

        return null;
    }
}