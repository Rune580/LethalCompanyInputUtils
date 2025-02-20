using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using LethalCompanyInputUtils.BindingPathEnums;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Data;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Api;

public abstract class LcInputActions
{
    public const string UnboundKeyboardAndMouseIdentifier = "<InputUtils-Kbm-Not-Bound>";
    public const string UnboundGamepadIdentifier = "<InputUtils-Gamepad-Not-Bound>";
    
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

    protected LcInputActions() : this(Assembly.GetCallingAssembly().GetBepInPlugin()) {}

    protected LcInputActions(BepInPlugin? plugin)
    {
        Asset = ScriptableObject.CreateInstance<InputActionAsset>();
        Plugin = plugin ?? throw new InvalidOperationException();

        var mapBuilder = new InputActionMapBuilder(Id);

        var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _inputProps = new Dictionary<PropertyInfo, InputActionAttribute>();
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<InputActionAttribute>();
            if (attr is null)
                continue;

            if (prop.PropertyType != typeof(InputAction))
                continue;

            attr.ActionId ??= prop.Name;
            
            attr.GamepadPath ??= attr.GamepadControl.ToPath();
            if (string.IsNullOrEmpty(attr.GamepadPath))
                attr.GamepadPath = UnboundGamepadIdentifier;
            
            var kbmPath = string.IsNullOrWhiteSpace(attr.KbmPath) ? UnboundKeyboardAndMouseIdentifier : attr.KbmPath;

            mapBuilder.NewActionBinding()
                .WithActionId(attr.ActionId)
                .WithActionType(attr.ActionType)
                .WithBindingName(attr.Name)
                .WithKbmPath(kbmPath)
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

    internal string GetBindingOverridesPath(BindingOverrideType overrideType) => overrideType.GetJsonPath(Id);

    internal BindingOverrides GetCurrentBindingOverrides() => new(Asset.bindings);

    internal BindingOverrides GetBindingOverrides(BindingOverrideType overrideType)
    {
        var jsonPath = overrideType.GetJsonPath(Id);

        if (!File.Exists(jsonPath))
            return new BindingOverrides();

        try
        {
            return BindingOverrides.FromJson(File.ReadAllText(jsonPath));
        }
        catch (Exception e)
        {
            Logging.Error(e);
        }

        return new BindingOverrides();
    }

    internal void Save(BindingOverrideType overrideType)
    {
        var overrides = new BindingOverrides(Asset.bindings);
        File.WriteAllText(overrideType.GetJsonPath(Id), overrides.AsJson());
    }

    internal void Load() => Asset.LoadOverridesFromControls(Id);
}