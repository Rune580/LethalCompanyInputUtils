using System;
using LethalCompanyInputUtils.Api.Composite;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;


public abstract class CompositeActionAttribute : Attribute
{
    /// <summary>
    /// Overrides the generated actionId for this <see cref="InputAction"/>.<remarks>Only needs to be unique within your mod</remarks>
    /// </summary>
    public string? ActionId { get; set; }
    
    public InputActionType ActionType { get; set; } = InputActionType.Value;
    
    internal abstract CompositeActionBindingBuilder BuildInto(InputActionBindingBuilder actionBuilder);
}