using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace LethalCompanyInputUtils.Data;

public class BindingOverrides
{
    public List<BindingOverride> Overrides { get; set; } = [];

    public BindingOverrides(IEnumerable<InputBinding> bindings)
    {
        foreach (var binding in bindings)
        {
            if (!binding.hasOverrides)
                continue;

            var bindingOverride = new BindingOverride
            {
                Action = binding.action,
                Path = binding.overridePath,
            };
            
            Overrides.Add(bindingOverride);
        }
    }

    public void LoadInto(InputActionAsset asset)
    {
        foreach (var bindingOverride in Overrides)
        {
            var action = asset.FindAction(bindingOverride.Action);

            action?.ApplyBindingOverride(new InputBinding(bindingOverride.Path));
        }
    }
}