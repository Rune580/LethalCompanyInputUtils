using System;
using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.BindingPathEnums;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api.Composite;

public abstract class CompositeActionBindingBuilder<TAxisEnum, TBuilder>
    where TAxisEnum : Enum
    where TBuilder : CompositeActionBindingBuilder<TAxisEnum, TBuilder>
{
    private readonly InputActionMapBuilder _mapBuilder;
    private readonly InputActionBindingBuilder _actionBuilder;

    private readonly Dictionary<TAxisEnum, string> _kbmAxisBindings = new();
    private readonly Dictionary<TAxisEnum, string?> _kbmAxisProcessors = new();
    private readonly Dictionary<TAxisEnum, string> _gamepadAxisBindings = new();
    private readonly Dictionary<TAxisEnum, string?> _gamepadAxisProcessors = new();

    private string? _interactions;
    private string? _processors;
    
    protected abstract string Composite { get; }
    
    internal CompositeActionBindingBuilder(InputActionMapBuilder mapBuilder, InputActionBindingBuilder actionBuilder)
    {
        _mapBuilder = mapBuilder;
        _actionBuilder = actionBuilder;

        foreach (var axis in EnumUtils.EnumerateValues<TAxisEnum>())
        {
            _kbmAxisBindings[axis] = "";
            _kbmAxisProcessors[axis] = "";
            _gamepadAxisBindings[axis] = "";
            _gamepadAxisProcessors[axis] = "";
        }
    }
    
    public TBuilder WithKbmAxisBinding(TAxisEnum axis, MouseControl control, string? processors = null) =>
        WithKbmAxisBinding(axis, control.ToPath(), processors);
    
    public TBuilder WithKbmAxisBinding(TAxisEnum axis, KeyboardControl control, string? processors = null) =>
        WithKbmAxisBinding(axis, control.ToPath(), processors);

    public TBuilder WithKbmAxisBinding(TAxisEnum axis, string path, string? processors = null)
    {
        _kbmAxisBindings[axis] = path;
        _kbmAxisProcessors[axis] = processors;
        return (TBuilder)this;
    }

    public TBuilder WithGamepadAxisBinding(TAxisEnum axis, GamepadControl control, string? processors = null) =>
        WithGamepadAxisBinding(axis, control.ToPath(), processors);

    public TBuilder WithGamepadAxisBinding(TAxisEnum axis, string path, string? processors = null)
    {
        _gamepadAxisBindings[axis] = path;
        _gamepadAxisProcessors[axis] = processors;
        return (TBuilder)this;
    }

    public TBuilder WithInteractions(string? interactions)
    {
        _interactions = interactions;
        return (TBuilder)this;
    }

    public TBuilder WithProcessors(string? processors)
    {
        _processors = processors;
        return (TBuilder)this;
    }

    public InputAction Finish()
    {
        var action = _actionBuilder.Finish();
        
        var syntax = action.AddCompositeBinding(Composite, _interactions, _processors);

        foreach (var axis in EnumUtils.EnumerateValues<TAxisEnum>())
        {
            var kbmBinding = _kbmAxisBindings[axis];
            var kbmProcessors = _kbmAxisProcessors[axis];
            var gamepadBinding = _gamepadAxisBindings[axis];
            var gamepadProcessors = _gamepadAxisProcessors[axis];

            var axisName = Enum.GetName(typeof(TAxisEnum), axis);

            if (!string.IsNullOrWhiteSpace(kbmBinding))
                syntax = syntax.With(axisName, kbmBinding, DeviceGroups.KeyboardAndMouse, kbmProcessors);

            if (!string.IsNullOrWhiteSpace(gamepadBinding))
                syntax = syntax.With(axisName, gamepadBinding, DeviceGroups.Gamepad, gamepadProcessors);
        }

        foreach (var binding in action.bindings.Where(binding => binding.isComposite || binding.isPartOfComposite))
            _mapBuilder.WithBinding(binding);
        
        return action;
    }
}