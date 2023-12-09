using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

public abstract class LcInputActions
{
    private readonly string _jsonPath;
    protected readonly InputActionAsset Asset;
    
    protected LcInputActions()
    {
        Asset = ScriptableObject.CreateInstance<InputActionAsset>();
        
        var plugin = Assembly.GetCallingAssembly().GetBepInPlugin();
        var modGuid = plugin.GUID;
        _jsonPath = Path.Combine(Paths.ControlsPath, $"{modGuid}.json");

        var mapBuilder = new InputActionMapBuilder(modGuid);

        var props = GetType().GetProperties();

        var inputProps = new Dictionary<PropertyInfo, InputActionAttribute>();
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<InputActionAttribute>();
            if (attr is null)
                continue;

            if (prop.PropertyType != typeof(InputAction))
                continue;
            
            var actionBuilder = mapBuilder.NewActionBinding();

            actionBuilder
                .WithActionName(attr.Action)
                .WithActionType(attr.ActionType)
                .WithBindingName(attr.Name)
                .WithKbmPath(attr.KbmPath)
                .WithGamepadPath(attr.GamepadPath)
                .Finish();

            inputProps[prop] = attr;
        }

        Asset.AddActionMap(mapBuilder.Build());
        Asset.Enable();

        foreach (var (prop, attr) in inputProps)
            prop.SetValue(this, Asset.FindAction(attr.Action));
    }

    public void Enable()
    {
        Asset.Enable();
    }

    internal void Save()
    {
        File.WriteAllText(_jsonPath, Asset.SaveBindingOverridesAsJson());
    }

    internal void Load()
    {
        Asset.LoadBindingOverridesFromJson(_jsonPath);
    }
}