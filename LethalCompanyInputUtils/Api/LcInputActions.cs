using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using LethalCompanyInputUtils.Data;
using LethalCompanyInputUtils.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

public abstract class LcInputActions
{
    private readonly string _jsonPath;
    private readonly InputActionReference[] _actionRefs;
    protected readonly InputActionAsset Asset;

    internal bool Loaded = false;

    internal bool WasEnabled { get; private set; }
    public bool Enabled => Asset.enabled;
    internal IReadOnlyCollection<InputActionReference> ActionRefs => _actionRefs;
    internal string Id => $"{Plugin.GUID}.{GetType().Name}";
    public BepInPlugin Plugin { get; }
    
    protected LcInputActions()
    {
        Asset = ScriptableObject.CreateInstance<InputActionAsset>();
        Plugin = Assembly.GetCallingAssembly().GetBepInPlugin() ?? throw new InvalidOperationException();
        
        var modGuid = Plugin.GUID;
        _jsonPath = Path.Combine(FsUtils.ControlsDir, $"{modGuid}.json");

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
                .WithKbmInteractions(attr.KbmInteractions)
                .WithGamepadInteractions(attr.GamepadInteractions)
                .Finish();

            inputProps[prop] = attr;
        }

        Asset.AddActionMap(mapBuilder.Build());
        Asset.Enable();

        var actionRefs = new List<InputActionReference>();
        foreach (var (prop, attr) in inputProps)
        {
            var action = Asset.FindAction(attr.Action);
            prop.SetValue(this, action);
            
            actionRefs.Add(InputActionReference.Create(action));
        }

        _actionRefs = actionRefs.ToArray();
        
        LcInputActionApi.RegisterInputActions(this);
    }

    public void Enable()
    {
        WasEnabled = Asset.enabled;
        Asset.Enable();
    }

    public void Disable()
    {
        WasEnabled = Asset.enabled;
        Asset.Disable();
    }

    internal void Save()
    {
        var overrides = new BindingOverrides(Asset.bindings);
        File.WriteAllText(_jsonPath, JsonConvert.SerializeObject(overrides));
    }

    internal void Load()
    {
        if (!File.Exists(_jsonPath))
            return;
        
        try
        {
            var overrides = BindingOverrides.FromJson(File.ReadAllText(_jsonPath));
            overrides.LoadInto(Asset);
        }
        catch (Exception e)
        {
            Logging.Logger.LogError(e);
        }
    }
}