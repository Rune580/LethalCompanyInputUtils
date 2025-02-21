using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Utils;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Data;

internal class VanillaInputActions
{
    private const string FileName = "LethalCompanyOverrides";
    
    [field: AllowNull, MaybeNull]
    public static VanillaInputActions Instance
    {
        get
        {
            field ??= new VanillaInputActions();
            return field;
        }
        internal set;
    }
    
    public readonly InputActionAsset Asset;

    public readonly IReadOnlyCollection<InputActionReference> ActionRefs;

    internal VanillaInputActions()
    {
        Asset = IngamePlayerSettings.Instance.playerInput.actions;

        ActionRefs = Asset.actionMaps
            .SelectMany(map => map.actions)
            .Select(InputActionReference.Create)
            .ToArray();
    }
    
    public string GetBindingOverridesPath(BindingOverrideType overrideType) => overrideType.GetJsonPath(FileName);
    
    internal BindingOverrides GetBindingOverrides(BindingOverrideType overrideType)
    {
        if (overrideType is BindingOverrideType.Global)
            return GetGlobalBindingOverridesFromVanilla();
        
        var jsonPath = overrideType.GetJsonPath(FileName);

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

    public void Load()
    {
        var overridePriority = InputUtilsConfig.bindingOverridePriority.Value;

        var globalOverrides = GetGlobalBindingOverridesFromVanilla();
        
        switch (overridePriority)
        {
            case BindingOverridePriority.GlobalThenLocal:
                Asset.LoadOverridesFromControls(FileName, BindingOverrideType.Local, true);
                globalOverrides.LoadInto(Asset);
                break;
            case BindingOverridePriority.LocalThenGlobal:
                Asset.RemoveAllBindingOverrides();
                globalOverrides.LoadInto(Asset);
                Asset.LoadOverridesFromControls(FileName, BindingOverrideType.Local, false);
                break;
            case BindingOverridePriority.GlobalOnly:
                Asset.RemoveAllBindingOverrides();
                globalOverrides.LoadInto(Asset);
                break;
            case BindingOverridePriority.LocalOnly:
                Asset.LoadOverridesFromControls(FileName, BindingOverrideType.Local, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(overridePriority), overridePriority, null);
        }
    }

    private BindingOverrides GetGlobalBindingOverridesFromVanilla()
    {
        if (!BindingOverrides.TryFromUnityInputJson(Asset, IngamePlayerSettings.Instance.settings.keyBindings, out var bindingOverrides))
            return new BindingOverrides();

        return bindingOverrides ?? new BindingOverrides();
    }
}