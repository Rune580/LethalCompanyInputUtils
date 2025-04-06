using System;
using LethalCompanyInputUtils.Api.Composite;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

[AttributeUsage(AttributeTargets.Property)]
public class CompositeVector2DActionAttribute : CompositeActionAttribute
{
    /// <summary>
    /// Overrides the generated actionId for this <see cref="InputAction"/>.<remarks>Only needs to be unique within your mod</remarks>
    /// </summary>
    public string? ActionId { get; set; }
    
    
    public CompositeVectorMode Mode { get; set; } = CompositeVectorMode.DigitalNormalized;
    
    internal override void BuildInto(InputActionBindingBuilder actionBuilder)
    {
        // actionBuilder.AddVector2DCompositeBinding(Mode)
    }
}