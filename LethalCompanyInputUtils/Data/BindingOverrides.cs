using System;
using System.Collections.Generic;
using LethalCompanyInputUtils.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
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
            
            var group = bindingOverride.groups;

            if (string.IsNullOrEmpty(group))
            {
                action?.ApplyBindingOverride(bindingOverride.path, path: bindingOverride.origPath);
            }
            else
            {
                action?.ApplyBindingOverride(bindingOverride.path, group);
            }
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
                
                var group = bindingOverride.groups;

                if (string.IsNullOrEmpty(group))
                {
                    actionRef.action.ApplyBindingOverride(bindingOverride.path, path: bindingOverride.origPath);
                }
                else
                {
                    actionRef.action.ApplyBindingOverride(bindingOverride.path, group);
                }
                
                break;
            }
        }
    }

    public bool ContainsOverrideFor(RemappableKey key)
    {
        var actionRef = key.currentInput;
        var action = actionRef.action;
        
        var bindIndex = key.GetRebindingIndex();
        
        if (bindIndex < 0 || bindIndex >= action.bindings.Count)
            return false;
        
        var binding = action.bindings[bindIndex];
        
        foreach (var bindingOverride in overrides)
        {
            if (action.name != bindingOverride.action)
                continue;
            
            var origPath = bindingOverride.origPath;
            var group = bindingOverride.groups;
            
            if (!string.IsNullOrEmpty(group) && binding.groups.Contains(group) && binding.path == origPath)
                return true;

            if (binding.path == origPath)
                return true;
        }
        
        return false;
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

    public static bool TryFromUnityInputJson(InputActionAsset asset, string json, out BindingOverrides? overrides)
    {
        overrides = null;
        
        if (string.IsNullOrEmpty(json))
            return false;
        
        try
        {
            var bindings = new List<InputBinding>();

            foreach (var bindingOverrideJson in JsonUtility.FromJson<InputActionMap.BindingOverrideListJson>(json)
                         .bindings)
            {
                if (string.IsNullOrEmpty(bindingOverrideJson.id))
                {
                    Debug.LogWarning("Could not override binding as no existing binding was found with the id: " +
                                     bindingOverrideJson.id);
                    continue;
                }

                var bindingMask = new InputBinding
                {
                    m_Id = bindingOverrideJson.id
                };

                var num = asset.FindBinding(bindingMask, out var action);

                if (num == -1)
                    continue;

                var binding = InputActionMap.BindingOverrideJson.ToBinding(bindingOverrideJson);
                
                binding.id = action.id;
                binding.action = action.bindings[num].action;
                binding.path = action.bindings[num].path;
                
                bindings.Add(binding);
            }

            overrides = new BindingOverrides(bindings);
            return true;
        }
        catch (Exception e)
        {
            Logging.Error(e);
            return false;
        }
    }
}