using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Data;
using LethalCompanyInputUtils.Utils;
using Newtonsoft.Json;
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

    protected LcInputActions()
    {
        Asset = ScriptableObject.CreateInstance<InputActionAsset>();
        Plugin = Assembly.GetCallingAssembly().GetBepInPlugin() ?? throw new InvalidOperationException();

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
            attr.GamepadPath ??= UnboundGamepadIdentifier;
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

    internal void Load()
    {
        var overridePriority = InputUtilsConfig.BindingOverridePriority;
        
        switch (overridePriority)
        {
            case BindingOverridePriority.GlobalThenLocal:
                Load(BindingOverrideType.Local, true);
                Load(BindingOverrideType.Global, false);
                break;
            case BindingOverridePriority.LocalThenGlobal:
                Load(BindingOverrideType.Global, true);
                Load(BindingOverrideType.Local, false);
                break;
            case BindingOverridePriority.GlobalOnly:
                Load(BindingOverrideType.Global, true);
                break;
            case BindingOverridePriority.LocalOnly:
                Load(BindingOverrideType.Local, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(overridePriority), overridePriority, null);
        }
    }

    internal void Load(BindingOverrideType overrideType, bool removeExistingOverrides)
    {
        try
        {
            ApplyMigrations();
        }
        catch (Exception e)
        {
            Logging.Error("Got error when applying migrations, skipping...");
            Logging.Error(e);
        }

        var jsonPath = overrideType.GetJsonPath(Id);

        if (!File.Exists(jsonPath))
            return;

        try
        {
            var overrides = BindingOverrides.FromJson(File.ReadAllText(jsonPath));

            if (removeExistingOverrides)
                Asset.RemoveAllBindingOverrides();
            
            overrides.LoadInto(Asset);
        }
        catch (Exception e)
        {
            Logging.Error(e);
        }
    }

    private void ApplyMigrations()
    {
        // 0.7.0 added a global overrides locations, anything beforehand should assume local path.
        var pre070JsonPath = BindingOverrideType.Local.GetJsonPath(Id);
        
        // The overrides json path was changed to be nested in the config folder so that they could be packaged with modpacks.
        var pre041JsonPath = Path.Combine(FsUtils.Pre041ControlsDir, $"{Id}.json");
        if (File.Exists(pre041JsonPath) && !File.Exists(pre070JsonPath))
            File.Move(pre041JsonPath, pre070JsonPath);

        // If there isn't an existing overrides json then there is no need to apply any migrations
        if (!File.Exists(pre070JsonPath))
            return;

        // Unbound binds now use a special identifier as leaving them blank prevented the overrides from actually taking effect.
        var hasPre043Overrides = File.ReadAllText(pre070JsonPath)
            .Replace(" ", "")
            .Contains("\"origPath\":\"\"");

        if (hasPre043Overrides)
        {
            var overrides = BindingOverrides.FromJson(File.ReadAllText(pre070JsonPath));

            for (var i = 0; i < overrides.overrides.Count; i++)
            {
                var bindingOverride = overrides.overrides[i];
                if (!string.IsNullOrEmpty(bindingOverride.origPath))
                    continue;

                if (bindingOverride.path is null)
                    continue;

                if (bindingOverride.path.StartsWith("<Keyboard>", StringComparison.InvariantCultureIgnoreCase)
                    || bindingOverride.path.StartsWith("<Mouse>", StringComparison.InvariantCultureIgnoreCase))
                {
                    bindingOverride.origPath = UnboundKeyboardAndMouseIdentifier;
                }
                else
                {
                    // It's possible someone may use a unique path, such as a DualSense touchpad
                    // so just assume that if their path wasn't either a <Keyboard> or <Mouse>, that it's probably a <Gamepad>
                    bindingOverride.origPath = UnboundGamepadIdentifier;
                }

                overrides.overrides[i] = bindingOverride;
            }

            File.WriteAllText(pre070JsonPath, JsonConvert.SerializeObject(overrides));
        }
        
        // 0.7.0 added groups to the BindingOverride schema for better device specific bind detection.
        var hasPre070Overrides = !File.ReadAllText(pre070JsonPath)
            .Replace(" ", "")
            .Contains("\"groups\":");

        if (hasPre070Overrides)
        {
            var overrides = BindingOverrides.FromJson(File.ReadAllText(pre070JsonPath));

            for (int i = 0; i < overrides.overrides.Count; i++)
            {
                var bindingOverride = overrides.overrides[i];
                if (string.IsNullOrEmpty(bindingOverride.origPath))
                    continue;

                if (bindingOverride.origPath.StartsWith("<Keyboard>", StringComparison.InvariantCultureIgnoreCase)
                    || bindingOverride.origPath.StartsWith("<Mouse>", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(bindingOverride.origPath, UnboundKeyboardAndMouseIdentifier))
                {
                    bindingOverride.groups = DeviceGroups.KeyboardAndMouse;
                }
                else
                {
                    bindingOverride.groups = DeviceGroups.Gamepad;
                }

                overrides.overrides[i] = bindingOverride;
            }
            
            File.WriteAllText(pre070JsonPath, JsonConvert.SerializeObject(overrides));
        }
    }
}