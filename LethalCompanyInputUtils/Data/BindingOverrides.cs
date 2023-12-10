using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Data;

[Serializable]
public class BindingOverrides
{
    public List<BindingOverride> overrides;

    private BindingOverrides()
    {
        overrides = new List<BindingOverride>();
    }
    
    public BindingOverrides(IEnumerable<InputBinding> bindings)
    {
        overrides = new List<BindingOverride>();
        foreach (var binding in bindings)
        {
            if (!binding.hasOverrides)
                continue;

            var bindingOverride = new BindingOverride
            {
                action = binding.action,
                origPath = binding.path,
                path = binding.overridePath
            };
            
            overrides.Add(bindingOverride);
        }
    }

    public void LoadInto(InputActionAsset asset)
    {
        foreach (var bindingOverride in overrides)
        {
            var action = asset.FindAction(bindingOverride.action);

            action?.ApplyBindingOverride(bindingOverride.path, path: bindingOverride.origPath);
        }
    }

    public static BindingOverrides FromJson(string json)
    {
        var overrides = new BindingOverrides();

        var node = JsonConvert.DeserializeObject<JObject>(json);
        var list = node!.GetValue(nameof(BindingOverrides.overrides))!;

        overrides.overrides = list.ToObject<List<BindingOverride>>()!;

        return overrides;
    }
}