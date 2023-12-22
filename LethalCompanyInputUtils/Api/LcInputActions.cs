using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly List<InputActionReference> _actionRefs = [];

    public InputActionAsset Asset { get; }
    internal InputActionAsset GetAsset() => Asset;

    internal bool Loaded = false;
    private readonly Dictionary<PropertyInfo, InputActionAttribute> _inputProps;

    internal bool WasEnabled { get; private set; }
    public bool Enabled => Asset.enabled;
    internal IReadOnlyCollection<InputActionReference> ActionRefs => _actionRefs;
    internal string Id => $"{Plugin.GUID}.{MapName}";
    public BepInPlugin Plugin { get; }

    protected virtual string MapName => GetType().Name;

    protected LcInputActions()
    {
        Asset = ScriptableObject.CreateInstance<InputActionAsset>();
        Plugin = Assembly.GetCallingAssembly().GetBepInPlugin() ?? throw new InvalidOperationException();

        _jsonPath = Path.Combine(FsUtils.ControlsDir, $"{Id}.json");

        var mapBuilder = new InputActionMapBuilder(Id);

        var props = GetType().GetProperties();
        _inputProps = new Dictionary<PropertyInfo, InputActionAttribute>();
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<InputActionAttribute>();
            if (attr is null)
                continue;

            if (prop.PropertyType != typeof(InputAction))
                continue;

            attr.ActionId ??= prop.Name;
            attr.GamepadPath ??= "";

            mapBuilder.NewActionBinding()
                .WithActionId(attr.ActionId)
                .WithActionType(attr.ActionType)
                .WithBindingName(attr.Name)
                .WithKbmPath(attr.KbmPath)
                .WithGamepadPath(attr.GamepadPath)
                .WithKbmInteractions(attr.KbmInteractions)
                .WithGamepadInteractions(attr.GamepadInteractions)
                .Finish();

            _inputProps[prop] = attr;
        }

        LcInputActionApi.RegisterInputActions(this, mapBuilder);
    }

    public virtual void CreateInputActions(in InputActionMapBuilder builder) { }

    public virtual void OnAssetLoaded() { }

    internal void BuildActionRefs()
    {
        foreach (var (prop, attr) in _inputProps)
        {
            var action = Asset.FindAction(attr.ActionId);
            prop.SetValue(this, action);
        }

        var refs = Asset.actionMaps
            .SelectMany(map => map.actions)
            .Select(InputActionReference.Create);

        _actionRefs.AddRange(refs);
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
        try
        {
            ApplyMigrations();
        }
        catch (Exception e)
        {
            Logging.Logger.LogError("Got error when applying migrations, skipping...");
            Logging.Logger.LogError(e);
        }

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

    private void ApplyMigrations()
    {
        var pre041JsonPath = Path.Combine(FsUtils.Pre041ControlsDir, $"{Id}.json");
        if (File.Exists(pre041JsonPath) && !File.Exists(_jsonPath))
            File.Move(pre041JsonPath, _jsonPath);
    }
}