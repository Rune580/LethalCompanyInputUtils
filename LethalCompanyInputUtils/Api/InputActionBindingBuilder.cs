using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

public class InputActionBindingBuilder
{
    private readonly InputActionMapBuilder _mapBuilder;
    
    private string? _actionName;
    
    private string? _kbmPath;
    private string? _gamepadPath;

    private string? _kbmInteractions;
    private string? _gamepadInteractions;
    
    private InputActionType _actionType = InputActionType.Value;
    private string? _name;

    internal InputActionBindingBuilder(InputActionMapBuilder mapBuilder)
    {
        _mapBuilder = mapBuilder;
    }

    public InputActionBindingBuilder WithActionName(string actionName)
    {
        _actionName = actionName;
        return this;
    }

    public InputActionBindingBuilder WithKbmPath(string kbmPath)
    {
        _kbmPath = kbmPath;
        return this;
    }
    
    public InputActionBindingBuilder WithGamepadPath(string gamepadPath)
    {
        _gamepadPath = gamepadPath;
        return this;
    }

    public InputActionBindingBuilder WithKbmInteractions(string? kbmInteractions)
    {
        _kbmInteractions = kbmInteractions;
        return this;
    }
    
    public InputActionBindingBuilder WithGamepadInteractions(string? gamepadInteractions)
    {
        _gamepadInteractions = gamepadInteractions;
        return this;
    }

    public InputActionBindingBuilder WithActionType(InputActionType actionType)
    {
        _actionType = actionType;
        return this;
    }

    public InputActionBindingBuilder WithBindingName(string? name)
    {
        _name = name;
        return this;
    }

    public InputAction Finish()
    {
        _name ??= _actionName;
        

        var action = new InputAction(_actionName, _actionType);
        _mapBuilder.WithAction(action);

        if (_kbmPath is not null)
            _mapBuilder.WithBinding(new InputBinding(_kbmPath, _actionName, interactions: _kbmInteractions, name: _name));
        
        if (_gamepadPath is not null)
            _mapBuilder.WithBinding(new InputBinding(_gamepadPath, _actionName, interactions: _gamepadInteractions, name: _name));

        return action;
    }
}