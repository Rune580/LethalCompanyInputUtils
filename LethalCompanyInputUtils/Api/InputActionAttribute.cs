using System;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

[AttributeUsage(AttributeTargets.Property)]
public class InputActionAttribute : Attribute
{
    [Obsolete("Prefer using the named optional params instead.")]
    public InputActionAttribute(string action, string kbmPath, string gamepadPath)
    {
        ActionId = action;
        KbmPath = kbmPath;
        GamepadPath = gamepadPath;
    }
    
    /// <param name="kbmPath">The default bind for Keyboard and Mouse devices</param>
    public InputActionAttribute(string kbmPath)
    {
        KbmPath = kbmPath;
    }
    
    public readonly string KbmPath;
    
    /// <summary>
    /// Overrides the generated actionId for this <see cref="InputAction"/>.<remarks>Only needs to be unique within your mod</remarks>
    /// </summary>
    public string? ActionId { get; set; }
    
    /// <summary>
    /// The default bind for Gamepad devices.
    /// </summary>
    public string? GamepadPath { get; set; }
    
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