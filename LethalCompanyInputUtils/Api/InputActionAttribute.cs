using System;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

[AttributeUsage(AttributeTargets.Property)]
public class InputActionAttribute(string action, string kbmPath, string gamepadPath) : Attribute
{
    public readonly string Action = action;
    public readonly string KbmPath = kbmPath;
    public readonly string GamepadPath = gamepadPath;

    public InputActionType ActionType { get; set; } = InputActionType.Button;
    
    /// <summary>
    /// Sets the interactions of the kbm binding.
    /// </summary>
    public string? KbmInteractions { get; set; }
    
    /// <summary>
    /// Sets the interactions of the gamepad binding.
    /// </summary>
    public string? GamepadInteractions { get; set; }
    
    /// <summary>
    /// Override the display name of the keybind in-game.
    /// </summary>
    public string? Name { get; set; }
}