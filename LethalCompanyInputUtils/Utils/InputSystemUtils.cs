using System.Linq;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Utils;

internal static class InputSystemUtils
{
    public static bool IsGamepadOnly(this InputAction action) => action.bindings.All(binding => !binding.IsKbmBind());

    public static bool IsKbmBind(this InputBinding binding)
    {
        var path = binding.effectivePath;
        
        if (string.Equals(path, LcInputActions.UnboundKeyboardAndMouseIdentifier))
            return true;

        if (path.StartsWith("<Keyboard>"))
            return true;

        if (path.StartsWith("<Mouse>"))
            return true;

        return false;
    }
}