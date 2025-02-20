using System;
using System.IO;
using System.Linq;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Data;
using Newtonsoft.Json;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Utils;

internal static class InputSystemUtils
{
    public static bool IsGamepadOnly(this InputAction action) => action.bindings.All(binding => !binding.IsKbmBind());

    public static bool IsKbmBind(this InputBinding binding) => string.Equals(binding.groups, DeviceGroups.KeyboardAndMouse);

    public static bool IsGamepadBind(this InputBinding binding) => string.Equals(binding.groups, DeviceGroups.Gamepad);

    public static RemappableKey? GetKbmKey(this InputActionReference actionRef)
    {
        var bindings = actionRef.action.bindings;
        
        for (var i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            if (!binding.IsKbmBind())
                continue;
            
            return new RemappableKey
            {
                ControlName = binding.name,
                currentInput = actionRef,
                rebindingIndex = i,
                gamepadOnly = false
            };
        }

        return null;
    }

    public static RemappableKey? GetGamepadKey(this InputActionReference actionRef)
    {
        var bindings = actionRef.action.bindings;

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            if (!binding.IsGamepadBind())
                continue;

            return new RemappableKey
            {
                ControlName = binding.name,
                currentInput = actionRef,
                rebindingIndex = i,
                gamepadOnly = true
            };
        }

        return null;
    }

    public static int GetRebindingIndex(this RemappableKey? key)
    {
        if (key is null)
            return -1;

        var action = key.currentInput.action;

        if (action.controls.Count == 0)
        {
            if (action.bindings.Count == 0)
                return -1;

            if (key.gamepadOnly && action.bindings.Count > 1)
                return 1;
            
            return 0;
        }

        return key.rebindingIndex < 0 ? action.GetBindingIndexForControl(action.controls[0]) : key.rebindingIndex;
    }

    public static void LoadOverridesFromControls(this InputActionAsset asset, string fileName)
    {
        var overridePriority = InputUtilsConfig.bindingOverridePriority.Value;
        
        switch (overridePriority)
        {
            case BindingOverridePriority.GlobalThenLocal:
                asset.LoadOverridesFromControls(fileName, BindingOverrideType.Local, true);
                asset.LoadOverridesFromControls(fileName, BindingOverrideType.Global, false);
                break;
            case BindingOverridePriority.LocalThenGlobal:
                asset.LoadOverridesFromControls(fileName, BindingOverrideType.Global, true);
                asset.LoadOverridesFromControls(fileName, BindingOverrideType.Local, false);
                break;
            case BindingOverridePriority.GlobalOnly:
                asset.LoadOverridesFromControls(fileName, BindingOverrideType.Global, true);
                break;
            case BindingOverridePriority.LocalOnly:
                asset.LoadOverridesFromControls(fileName, BindingOverrideType.Local, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(overridePriority), overridePriority, null);
        }
    }

    public static void LoadOverridesFromControls(this InputActionAsset asset, string fileName, BindingOverrideType overrideType, bool removeExistingOverrides)
    {
        try
        {
            ApplyMigrations(fileName);
        }
        catch (Exception e)
        {
            Logging.Error("Got error when applying migrations, skipping...");
            Logging.Error(e);
        }

        var jsonPath = overrideType.GetJsonPath(fileName);

        if (!File.Exists(jsonPath))
            return;

        try
        {
            var overrides = BindingOverrides.FromJson(File.ReadAllText(jsonPath));

            if (removeExistingOverrides)
                asset.RemoveAllBindingOverrides();
            
            overrides.LoadInto(asset);
        }
        catch (Exception e)
        {
            Logging.Error(e);
        }
    }
    
    public static void ApplyMigrations(string fileName)
    {
        // 0.7.0 added a global overrides locations, anything beforehand should assume local path.
        var pre070JsonPath = BindingOverrideType.Local.GetJsonPath(fileName);
        
        // The overrides json path was changed to be nested in the config folder so that they could be packaged with modpacks.
        var pre041JsonPath = Path.Combine(FsUtils.Pre041ControlsDir, $"{fileName}.json");
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
                    bindingOverride.origPath = LcInputActions.UnboundKeyboardAndMouseIdentifier;
                }
                else
                {
                    // It's possible someone may use a unique path, such as a DualSense touchpad
                    // so just assume that if their path wasn't either a <Keyboard> or <Mouse>, that it's probably a <Gamepad>
                    bindingOverride.origPath = LcInputActions.UnboundGamepadIdentifier;
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
                    || string.Equals(bindingOverride.origPath, LcInputActions.UnboundKeyboardAndMouseIdentifier))
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