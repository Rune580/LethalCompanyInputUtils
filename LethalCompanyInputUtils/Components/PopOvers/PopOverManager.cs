using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Localization;
using LethalCompanyInputUtils.Utils;
using UnityEngine;

namespace LethalCompanyInputUtils.Components.PopOvers;

[RequireComponent(typeof(RectTransform))]
public class PopOverManager : MonoBehaviour
{
    private static PopOverManager? _instance;
    private static readonly List<PopOverTrigger> Triggers = [];
    
    public PopOver? popOver;

    private void OnEnable()
    {
        if (_instance is not null)
        {
            Logging.Warn("There are too many PopOverManager instances! Attempting to clean up...");
            DestroyImmediate(_instance.gameObject);
            _instance = null;
        }
        
        _instance = this;
        Triggers.Clear();
        
        ClearPopOver();
    }

    private void OnDisable()
    {
        _instance = null;
        Triggers.Clear();
    }

    private void OnDestroy()
    {
        _instance = null;
        Triggers.Clear();
    }

    private void HandlePopOverTrigger(PopOverTrigger trigger)
    {
        if (popOver is null)
            return;

        if (trigger.target is null)
            return;
        
        popOver.SetTarget(trigger.target, trigger.placement);
        popOver.SetText(trigger.textIsLangToken ? LocaleManager.GetString(trigger.text) : trigger.text);
    }

    private void ClearPopOver()
    {
        if (popOver is null)
            return;
        
        popOver.ClearTarget();
    }

    public static void AddHotTrigger(PopOverTrigger trigger)
    {
        if (_instance is null)
            return;

        if (Triggers.Contains(trigger))
            return;
        
        Triggers.Add(trigger);
        
        _instance.HandlePopOverTrigger(trigger);
    }

    public static void RemoveHotTrigger(PopOverTrigger trigger)
    {
        if (_instance is null)
            return;

        Triggers.Remove(trigger);

        if (Triggers.Count == 0)
        {
            _instance.ClearPopOver();
            return;
        }

        _instance.HandlePopOverTrigger(Triggers.Last());
    }
}