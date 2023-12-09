using System;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InputActionAttribute(string action, string kbmPath, string gamepadPath) : Attribute
{
    public readonly string Action = action;
    public readonly string KbmPath = kbmPath;
    public readonly string GamepadPath = gamepadPath;

    public InputActionType ActionType { get; set; } = InputActionType.Button;
    public string? Name { get; set; }
}