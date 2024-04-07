using System;
using LethalCompanyInputUtils.BindingPathEnums;
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
    
    /// <param name="keyboardControl">The default Keyboard bind</param>
    public InputActionAttribute(KeyboardControl keyboardControl)
    {
        KbmPath = keyboardControl.ToPath();
    }
    
    /// <param name="mouseControl">The default Mouse bind</param>
    public InputActionAttribute(MouseControl mouseControl)
    {
        KbmPath = mouseControl.ToPath();
    }
    
    public readonly string KbmPath;
    
    /// <summary>
    /// Overrides the generated actionId for this <see cref="InputAction"/>.<remarks>Only needs to be unique within your mod</remarks>
    /// </summary>
    public string? ActionId { get; set; }
    
    /// <summary>
    /// The default bind for Gamepad devices.
    /// For using an GamepadControl enum, use <see cref="GamepadControl"/> instead.
    /// <remarks>Takes priority when both this and <see cref="GamepadControl"/> are set.</remarks>  
    /// </summary>
    public string? GamepadPath { get; set; }
    
    /// <summary>
    /// The default bind for Gamepad devices.
    /// For using a string path, use <see cref="GamepadPath"/> instead.
    /// </summary>
    public GamepadControl GamepadControl { get; set; }
    
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