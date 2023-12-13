using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

public class InputActionMapBuilder(string mapName)
{
    private readonly InputActionMap _actionMap = new(mapName);

    public InputActionMapBuilder WithAction(InputAction action)
    {
        _actionMap.AddAction(action.name, action.type);
        return this;
    }

    public InputActionMapBuilder WithBinding(InputBinding binding)
    {
        _actionMap.AddBinding(binding);
        return this;
    }

    public InputActionBindingBuilder NewActionBinding()
    {
        return new InputActionBindingBuilder(this);
    }

    internal InputActionMap Build()
    {
        return _actionMap;
    }
}