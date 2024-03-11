using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace LethalCompanyInputUtils.Lib.Search;

internal static class ControlsDb
{
    private static readonly HashSet<InputControlLayout.ControlItem> ControlItems = [];

    public static void Init()
    {
        if (ControlItems.Count != 0)
            return;

        RegisterControlsFromLayout<Keyboard>();
        RegisterControlsFromLayout<Mouse>();
        RegisterControlsFromLayout<Gamepad>();
        RegisterControlsFromLayout("InputUtilsExtendedMouse");
    }
    
    private static void RegisterControlsFromLayout<TLayout>()
        where TLayout : InputControl
    {
        var layout = InputSystem.LoadLayout<TLayout>();
        AddControls(layout.controls);
    }
    
    private static void RegisterControlsFromLayout(string layoutName)
    {
        var layout = InputSystem.LoadLayout(layoutName);
        AddControls(layout.controls);
    }

    private static void AddControls(IEnumerable<InputControlLayout.ControlItem> controlItems)
    {
        foreach (var item in controlItems)
            ControlItems.Add(item);
    }

    public static InputControlLayout.ControlItem[] FindBestMatchingControls(string search, int maxEntries = 5)
    {
         var results = ControlItems.Where(item => item.name.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase))
             .OrderBy(item =>
             {
                 var controlName = item.name.ToString();
                 var score = Mathf.Abs(controlName.Length - search.Length) + 1;

                 if (string.Equals(controlName, search, StringComparison.InvariantCultureIgnoreCase))
                     score = 0;

                 return score;

             })
             .Take(maxEntries)
             .ToArray();

         return results;
    }
}