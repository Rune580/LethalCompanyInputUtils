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
            if (field is null)
                throw new InvalidOperationException("VanillaInputActions is not yet initialized!");

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
        var jsonPath = overrideType.GetJsonPath(FileName);

        if (!File.Exists(jsonPath))
            return overrideType is BindingOverrideType.Global ? GetGlobalBindingOverridesFromVanilla() : new BindingOverrides();

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

    public void Load() => Asset.LoadOverridesFromControls(FileName);

    private BindingOverrides GetGlobalBindingOverridesFromVanilla()
    {
        if (!BindingOverrides.TryFromUnityInputJson(Asset, IngamePlayerSettings.Instance.settings.keyBindings, out var bindingOverrides))
            return new BindingOverrides();

        return bindingOverrides ?? new BindingOverrides();
    }
}