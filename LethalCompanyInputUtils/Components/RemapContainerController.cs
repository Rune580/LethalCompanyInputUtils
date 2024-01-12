using System.Collections.Generic;
using System.Linq;
using LethalCompanyInputUtils.Components.Section;
using UnityEngine;

namespace LethalCompanyInputUtils.Components;

public class RemapContainerController : MonoBehaviour
{
    public BindsListController? bindsList;
    public SectionListController? sectionList;

    public List<RemappableKey> baseGameKeys = [];

    private void Awake()
    {
        if (bindsList is null)
            bindsList = GetComponentInChildren<BindsListController>();

        if (sectionList is null)
            sectionList = GetComponentInChildren<SectionListController>();
        
        bindsList.OnSectionChanged.AddListener(HandleSectionChanged);
    }

    public void JumpTo(int sectionIndex)
    {
        if (bindsList is null)
            return;
        
        bindsList.JumpTo(sectionIndex);
    }

    public void LoadUi()
    {
        GenerateBaseGameSection();
        GenerateApiSections();
        FinishUi();
    }

    private void GenerateBaseGameSection()
    {
        if (bindsList is null || sectionList is null)
            return;
        
        // <Control Name, (Keyboard/Mouse Key, Gamepad Key)>
        var pairedKeys = new Dictionary<string, (RemappableKey?, RemappableKey?)>();
        
        foreach (var baseGameKey in baseGameKeys)
        {
            RemappableKey? kbmKey = null;
            RemappableKey? gamepadKey = null;
            
            // For some reason LethalCompany has multiple controls that do the same thing but for either Kbm or Gamepad.
            // So we try to de-duplicate the controls by *visually* merging them into the same entry.
            // This results in the controls being functionally the exact same internally while also providing better UX.
            
            // Some of the control names have different casing.
            var controlName = baseGameKey.ControlName.ToLower();
            
            if (controlName.StartsWith("walk")) // Kbm uses "walk" while gamepad uses "move", replace kbm name with gamepad name for matching.
                controlName = "move" + controlName[4..];

            // The control "Item primary use" uses different casing than the other 2 "Item ... use"s' ("Item Secondary use", "Item Tertiary use"),
            // So I'm doing myself a favor and fixing its casing to match the other 2.
            baseGameKey.ControlName = baseGameKey.ControlName.Replace("primary", "Primary");
            
            if (pairedKeys.TryGetValue(controlName, out var keyPair))
            {
                if (baseGameKey.gamepadOnly)
                {
                    kbmKey = keyPair.Item1;
                }
                else
                {
                    gamepadKey = keyPair.Item2;
                }
            }

            if (baseGameKey.gamepadOnly)
            {
                gamepadKey = baseGameKey;
            }
            else
            {
                kbmKey = baseGameKey;
            }

            pairedKeys[controlName] = (kbmKey, gamepadKey);
        }
        
        bindsList.AddSection("Lethal Company");
        sectionList.AddSection("Lethal Company");
        foreach (var (_, (kbmKey, gamepadKey)) in pairedKeys)
            bindsList.AddBinds(kbmKey, gamepadKey, true);
    }

    private void GenerateApiSections()
    {
        if (bindsList is null || sectionList is null)
            return;
        
        foreach (var lcInputActions in LcInputActionApi.InputActions)
        {
            if (lcInputActions.Loaded)
                continue;
            
            bindsList.AddSection(lcInputActions.Plugin.Name);
            sectionList.AddSection(lcInputActions.Plugin.Name);
            
            foreach (var actionRef in lcInputActions.ActionRefs)
            {
                var controlName = actionRef.action.bindings.First().name;
                var kbmKey = new RemappableKey
                {
                    ControlName = controlName,
                    currentInput = actionRef,
                    rebindingIndex = 0,
                    gamepadOnly = false
                };
                
                var gamepadKey = new RemappableKey
                {
                    ControlName = controlName,
                    currentInput = actionRef,
                    rebindingIndex = 1,
                    gamepadOnly = true
                };
                
                bindsList.AddBinds(kbmKey, gamepadKey);
            }
            
            lcInputActions.Loaded = true;
        }
    }

    private void FinishUi()
    {
        if (bindsList is null)
            return;
        
        bindsList.AddFooter();
        
        JumpTo(0);
    }
    
    private void HandleSectionChanged(int sectionIndex)
    {
        if (sectionList is null || bindsList is null)
            return;
        
        sectionList.SelectSection(sectionIndex);
    }

    private void OnEnable()
    {
        JumpTo(0);
    }

    private void OnDisable()
    {
        LcInputActionApi.ReEnableFromRebind();
    }
}