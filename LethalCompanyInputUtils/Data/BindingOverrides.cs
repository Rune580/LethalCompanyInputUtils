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

    public BindingOverrides()
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
                path = binding.overridePath,
                groups = binding.groups
            };
            
            overrides.Add(bindingOverride);
        }
    }

    public void LoadInto(InputActionAsset asset)
    {
        foreach (var bindingOverride in overrides)
        {
            var action = asset.FindAction(bindingOverride.action);

            action?.ApplyBindingOverride(bindingOverride.path, group: bindingOverride.groups);
        }
    }

    public void LoadInto(IReadOnlyCollection<InputActionReference> actionRefs)
    {
        foreach (var bindingOverride in overrides)
        {
            foreach (var actionRef in actionRefs)
            {
                if (actionRef.action.name != bindingOverride.action)
                    continue;
                
                actionRef.action.ApplyBindingOverride(bindingOverride.path, group: bindingOverride.groups);
                break;
            }
        }
    }

    public string AsJson() => JsonConvert.SerializeObject(this);

    public static BindingOverrides FromJson(string json)
    {
        var overrides = new BindingOverrides();

        var node = JsonConvert.DeserializeObject<JObject>(json);
        var list = node!.GetValue(nameof(BindingOverrides.overrides))!;

        overrides.overrides = list.ToObject<List<BindingOverride>>()!;

        return overrides;
    }
}