using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Components.Section;
using LethalCompanyInputUtils.Components.Switch;
using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Data;
using LethalCompanyInputUtils.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components;

public class RemapContainerController : MonoBehaviour
{
    public BindingOverrideContextSwitch? contextSwitch;
    public BindsListController? bindsList;
    public SectionListController? sectionList;
    public Button? backButton;
    public Button? legacyButton;
    public GameObject? legacyHolder;
    
    public List<RemappableKey> baseGameKeys = [];

    private readonly List<ContextBindingOverride> _contextBindingOverrides = [];
    
    internal int LayerShown;
    
    private void Awake()
    {
        if (contextSwitch is null)
            contextSwitch = GetComponentInChildren<BindingOverrideContextSwitch>();
        
        if (bindsList is null)
            bindsList = GetComponentInChildren<BindsListController>();

        if (sectionList is null)
            sectionList = GetComponentInChildren<SectionListController>();
        
        bindsList.OnSectionChanged.AddListener(HandleSectionChanged);
        
        contextSwitch.onBindingOverrideContextChanged.AddListener(OnBindingOverrideContextChanged);

        LcInputActionApi.ContainerInstance = this;
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

    public void OnSetToDefault()
    {
        RebindButton.ResetAllToDefaults();
    }

    public void HideHighestLayer()
    {
        if (backButton is null || legacyHolder is null)
            return;
        
        if (LayerShown > 1)
        {
            legacyHolder.SetActive(false);
            LayerShown--;
            return;
        }

        if (LayerShown > 0)
            backButton.onClick.Invoke();
    }

    public void ShowLegacyUi()
    {
        if (!isActiveAndEnabled)
            return;

        if (legacyHolder is null)
            return;
        
        legacyHolder.SetActive(true);
        LayerShown++;
    }

    private void GenerateApiSections()
    {
        if (bindsList is null || sectionList is null)
            return;

        var pluginGroupedActions = LcInputActionApi.InputActions.GroupBy(lc => lc.Plugin.Name);
        
        foreach (var pluginActions in pluginGroupedActions)
        {
            bindsList.AddSection(pluginActions.Key);
            sectionList.AddSection(pluginActions.Key);
            
            foreach (var lcInputAction in pluginActions)
            {
                if (lcInputAction.Loaded)
                    continue;
                
                _contextBindingOverrides.Add(new ContextBindingOverride(lcInputAction));
            
                foreach (var actionRef in lcInputAction.ActionRefs)
                {
                    var kbmKey = actionRef.GetKbmKey();
                    var gamepadKey = actionRef.GetGamepadKey();
                
                    bindsList.AddBinds(kbmKey, gamepadKey);
                }
            
                lcInputAction.Loaded = true;
            }
        }
    }

    private void FinishUi()
    {
        if (bindsList is null)
            return;
        
        bindsList.AddFooter();
        
        JumpTo(0);

        foreach (var binding in _contextBindingOverrides)
            binding.LoadOverrides(BindingOverrideType.Global, false);
    }
    
    private void HandleSectionChanged(int sectionIndex)
    {
        if (sectionList is null || bindsList is null)
            return;
        
        sectionList.SelectSection(sectionIndex);
    }
    
    private void OnBindingOverrideContextChanged(BindingOverrideType overrideType)
    {
        foreach (var binding in _contextBindingOverrides)
            binding.LoadOverrides(overrideType);
        
        RebindButton.ReloadGlyphs();
    }

    internal void SaveOverrides()
    {
        foreach (var binding in _contextBindingOverrides)
        {
            binding.SaveOverrides();
            binding.inputActions.Load();
        }
    }

    internal void DiscardOverrides()
    {
        foreach (var binding in _contextBindingOverrides)
            binding.DiscardOverrides();
    }
    
    private void OnEnable()
    {
        JumpTo(0);
        LayerShown = 1;
    }

    private void OnDisable()
    {
        if (contextSwitch is null)
            return;
        
        contextSwitch.SwitchToGlobal();
        
        LcInputActionApi.ReEnableFromRebind();
        LayerShown = 0;
    }

    private void OnDestroy()
    {
        LcInputActionApi.ContainerInstance = null;
        LayerShown = 0;
    }
    
    private class ContextBindingOverride
    {
        public readonly LcInputActions inputActions;
        
        private BindingOverrides _localOverrides;
        private BindingOverrides _globalOverrides;
        
        private BindingOverrideType _currentType;
        
        public ContextBindingOverride(LcInputActions inputActions)
        {
            this.inputActions = inputActions;

            _localOverrides = this.inputActions.GetBindingOverrides(BindingOverrideType.Local);
            _globalOverrides = this.inputActions.GetBindingOverrides(BindingOverrideType.Global);
        }

        public void LoadOverrides(BindingOverrideType overrideType, bool savePrev = true)
        {
            switch (overrideType)
            {
                case BindingOverrideType.Global:
                    if (savePrev && _currentType != overrideType)
                        _localOverrides = inputActions.GetCurrentBindingOverrides();
                    
                    inputActions.Asset.RemoveAllBindingOverrides();
                    _globalOverrides.LoadInto(inputActions.ActionRefs);
                    break;
                case BindingOverrideType.Local:
                    if (savePrev && _currentType != overrideType)
                        _globalOverrides = inputActions.GetCurrentBindingOverrides();
                    
                    inputActions.Asset.RemoveAllBindingOverrides();
                    _localOverrides.LoadInto(inputActions.ActionRefs);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overrideType), overrideType, null);
            }

            _currentType = overrideType;
        }

        public void SaveOverrides()
        {
            File.WriteAllText(inputActions.GetBindingOverridesPath(BindingOverrideType.Local), _localOverrides.AsJson());
            File.WriteAllText(inputActions.GetBindingOverridesPath(BindingOverrideType.Global), _globalOverrides.AsJson());
        }

        public void DiscardOverrides()
        {
            _localOverrides = inputActions.GetBindingOverrides(BindingOverrideType.Local);
            _globalOverrides = inputActions.GetBindingOverrides(BindingOverrideType.Global);
        }
    }
}