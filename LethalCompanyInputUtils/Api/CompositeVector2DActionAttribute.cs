using System;
using LethalCompanyInputUtils.Api.Composite;
using LethalCompanyInputUtils.BindingPathEnums;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class CompositeVector2DActionAttribute : CompositeActionAttribute
{
    private string _deviceGroup;

    private string _upPath;
    private string _downPath;
    private string _leftPath;
    private string _rightPath;
    
    public string? UpProcessors { get; set; }
    public string? DownProcessors { get; set; }
    public string? LeftProcessors { get; set; }
    public string? RightProcessors { get; set; }
    public string? Interactions { get; set; }
    
    public CompositeVectorMode Mode { get; set; } = CompositeVectorMode.DigitalNormalized;

    /// <summary>
    /// Add a composite binding to this <see cref="InputAction"/>.
    /// </summary>
    /// <param name="deviceGroup">See <see cref="Utils.DeviceGroups"/> for accepted values.</param>
    public CompositeVector2DActionAttribute(string upPath, string downPath, string leftPath, string rightPath, string deviceGroup)
    {
        _deviceGroup = deviceGroup;
        _upPath = upPath;
        _downPath = downPath;
        _leftPath = leftPath;
        _rightPath = rightPath;
        
        if (_deviceGroup != DeviceGroups.KeyboardAndMouse && _deviceGroup != DeviceGroups.Gamepad)
            throw new InvalidOperationException("Invalid deviceGroup specified!");
    }

    public CompositeVector2DActionAttribute(KeyboardControl up, KeyboardControl down, KeyboardControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
        DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, KeyboardControl down, KeyboardControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(KeyboardControl up, MouseControl down, KeyboardControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(KeyboardControl up, KeyboardControl down, MouseControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(KeyboardControl up, KeyboardControl down, KeyboardControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, KeyboardControl down, KeyboardControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(KeyboardControl up, MouseControl down, KeyboardControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(KeyboardControl up, KeyboardControl down, MouseControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, KeyboardControl down, MouseControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, KeyboardControl down, MouseControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, MouseControl down, MouseControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(KeyboardControl up, MouseControl down, MouseControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(KeyboardControl up, MouseControl down, MouseControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, MouseControl down, KeyboardControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, MouseControl down, KeyboardControl left, KeyboardControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(MouseControl up, MouseControl down, MouseControl left, MouseControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.KeyboardAndMouse) { }
    
    public CompositeVector2DActionAttribute(GamepadControl up, GamepadControl down, GamepadControl left, GamepadControl right)
        : this(up.ToPath(), down.ToPath(), left.ToPath(), right.ToPath(),
            DeviceGroups.Gamepad) { }
    
    internal override CompositeActionBindingBuilder BuildInto(InputActionBindingBuilder actionBuilder)
    {
        var compositeBuilder = actionBuilder.AddVector2DCompositeBinding(Mode);

        if (_deviceGroup == DeviceGroups.KeyboardAndMouse)
        {
            compositeBuilder.WithKbmAxisBinding(Vector2DAxis.Up, _upPath, UpProcessors)
                .WithKbmAxisBinding(Vector2DAxis.Down, _downPath, DownProcessors)
                .WithKbmAxisBinding(Vector2DAxis.Left, _leftPath, LeftProcessors)
                .WithKbmAxisBinding(Vector2DAxis.Right, _rightPath, RightProcessors);
        }
        else
        {
            compositeBuilder.WithGamepadAxisBinding(Vector2DAxis.Up, _upPath, UpProcessors)
                .WithGamepadAxisBinding(Vector2DAxis.Down, _downPath, DownProcessors)
                .WithGamepadAxisBinding(Vector2DAxis.Left, _leftPath, LeftProcessors)
                .WithGamepadAxisBinding(Vector2DAxis.Right, _rightPath, RightProcessors);
        }
        
        return compositeBuilder.WithInteractions(Interactions);
    }
}