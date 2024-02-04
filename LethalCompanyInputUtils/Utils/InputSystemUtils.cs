using System;
using System.Linq;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Utils;

internal static class InputSystemUtils
{
    public static bool IsGamepadOnly(this InputAction action) => action.bindings.All(binding => !binding.IsKbmBind());

    public static bool IsKbmBind(this InputBinding binding)
    {
        var path = binding.path;
        
        if (string.Equals(path, LcInputActions.UnboundKeyboardAndMouseIdentifier))
            return true;

        if (path.StartsWith("<Keyboard>", StringComparison.InvariantCultureIgnoreCase))
            return true;

        if (path.StartsWith("<Mouse>", StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;
    }
}