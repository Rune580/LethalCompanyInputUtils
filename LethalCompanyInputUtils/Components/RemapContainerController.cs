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
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components;

public class RemapContainerController : MonoBehaviour
{
    public BindingOverrideContextSwitch? contextSwitch;
    public TMP_Dropdown? overridePriorityDropdown;
    public BindsListController? bindsList;
    public SectionListController? sectionList;
    public Button? backButton;
    public Button? legacyButton;
    public GameObject? legacyHolder;
    
    public List<RemappableKey> baseGameKeys = [];

    private readonly List<BaseContextBindingOverride> _contextBindingOverrides = [];
    
    internal int LayerShown;
    
    private void Awake()
    {
        if (contextSwitch is null)
            contextSwitch = GetComponentInChildren<BindingOverrideContextSwitch>();
        
        if (bindsList is null)
            bindsList = GetComponentInChildren<BindsListController>();

        if (sectionList is null)
            sectionList = GetComponentInChildren<SectionListController>();

        if (overridePriorityDropdown is null)
            return;
        
        bindsList.OnSectionChanged.AddListener(HandleSectionChanged);
        
        contextSwitch.onBindingOverrideContextChanged.AddListener(OnBindingOverrideContextChanged);
        
        overridePriorityDropdown.onValueChanged.AddListener(OnOverridePriorityChanged);

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
        
        var contextBindingOverride = new VanillaContextBindingOverride();
        
        bindsList.AddSection("Lethal Company");
        sectionList.AddSection("Lethal Company");

        var actionRefMap = VanillaInputActions.Instance.ActionRefs
            .ToDictionary(actionRef => actionRef.action.name);
        
        foreach (var (_, (kbmKey, gamepadKey)) in pairedKeys)
        {
            if (kbmKey is not null)
            {
                if (actionRefMap.TryGetValue(kbmKey.currentInput.action.name, out var actionRef))
                    kbmKey.currentInput = actionRef;
            }
            
            if (gamepadKey is not null)
            {
                if (actionRefMap.TryGetValue(gamepadKey.currentInput.action.name, out var actionRef))
                    gamepadKey.currentInput = actionRef;
            }
            
            contextBindingOverride.AddKeys(kbmKey, gamepadKey);
            
            bindsList.AddBinds(kbmKey, gamepadKey, true);
        }
        
        _contextBindingOverrides.Add(contextBindingOverride);
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

                var contextBindingOverride = new ModContextBindingOverride(lcInputAction);
            
                foreach (var actionRef in lcInputAction.ActionRefs)
                {
                    var kbmKey = actionRef.GetKbmKey();
                    var gamepadKey = actionRef.GetGamepadKey();
                    
                    contextBindingOverride.AddKeys(kbmKey, gamepadKey);
                
                    bindsList.AddBinds(kbmKey, gamepadKey);
                }
                
                _contextBindingOverrides.Add(contextBindingOverride);
            
                lcInputAction.Loaded = true;
            }
        }
    }

    internal bool IsKeyOverriden(RemappableKey? key)
    {
        if (key is null)
            return false;

        var contextBindingOverrides = _contextBindingOverrides.Where(ctx => ctx.ContainsKey(key))
            .ToArray();
        
        if (contextBindingOverrides.Length == 0)
            return false;
        
        var contextBindingOverride = contextBindingOverrides[0];
        var currentContext = contextBindingOverride.CurrentType;
        
        var oppositeContext = currentContext is BindingOverrideType.Global
            ? BindingOverrideType.Local
            : BindingOverrideType.Global;

        var currentOverrides = contextBindingOverride.GetCurrentBindingOverrides();
        var oppositeOverrides = oppositeContext switch
        {
            BindingOverrideType.Global => contextBindingOverride.GlobalOverrides,
            BindingOverrideType.Local => contextBindingOverride.LocalOverrides,
            _ => throw new ArgumentOutOfRangeException()
        };

        var doesCurrentOverride = currentOverrides.ContainsOverrideFor(key);
        var doesOppositeOverride = oppositeOverrides.ContainsOverrideFor(key);

        switch (InputUtilsConfig.bindingOverridePriority.Value)
        {
            case BindingOverridePriority.GlobalThenLocal:
                if (currentContext is BindingOverrideType.Global)
                    return !doesCurrentOverride && doesOppositeOverride;
                return doesOppositeOverride;
            case BindingOverridePriority.LocalThenGlobal:
                if (currentContext is BindingOverrideType.Local)
                    return !doesCurrentOverride && doesOppositeOverride;
                return doesOppositeOverride;
            case BindingOverridePriority.GlobalOnly:
                if (currentContext is BindingOverrideType.Local)
                    return true;
                break;
            case BindingOverridePriority.LocalOnly:
                if (currentContext is BindingOverrideType.Global)
                    return true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return false;
    }

    private void FinishUi()
    {
        if (bindsList is null)
            return;
        
        bindsList.AddFooter();
        
        JumpTo(0);

        foreach (var binding in _contextBindingOverrides)
            binding.LoadOverrides(BindingOverrideType.Global);
        
        RebindButton.ReloadGlyphs();
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

    private void OnOverridePriorityChanged(int value)
    {
        var priority = (BindingOverridePriority)value;

        InputUtilsConfig.bindingOverridePriority.Value = priority;
        RebindButton.ReloadGlyphs();
    }

    internal void SaveOverrides()
    {
        for (var i = 0; i < _contextBindingOverrides.Count; i++)
        {
            var binding = _contextBindingOverrides[i];
            
            binding.SaveOverrides();
            binding.Reload();
            
            _contextBindingOverrides[i] = binding.Reset();
        }
    }

    internal void DiscardOverrides()
    {
        for (var i = 0; i < _contextBindingOverrides.Count; i++)
        {
            var binding = _contextBindingOverrides[i];
            
            binding.DiscardOverrides();
            binding.Reload();

            _contextBindingOverrides[i] = binding.Reset();
        }
    }
    
    private void OnEnable()
    {
        JumpTo(0);
        LayerShown = 1;
        
        if (contextSwitch is null)
            return;
        
        contextSwitch.SwitchToGlobal();

        // Force load global to fix issue where a user has "Local Only" priority and the local binds don't get unloaded properly, resulting in displaying incorrect keybinds.
        OnBindingOverrideContextChanged(BindingOverrideType.Global);

        if (overridePriorityDropdown is null)
            return;
        
        overridePriorityDropdown.SetValueWithoutNotify((int)InputUtilsConfig.bindingOverridePriority.Value);
    }

    private void OnDisable()
    {
        if (contextSwitch is null)
            return;
        
        LcInputActionApi.ReEnableFromRebind();
        LayerShown = 0;
    }

    private void OnDestroy()
    {
        LcInputActionApi.ContainerInstance = null;
        LayerShown = 0;
    }

    private abstract class BaseContextBindingOverride
    {
        public BindingOverrideType CurrentType { get; protected set; }
        protected abstract InputActionAsset Asset { get; }
        protected abstract IReadOnlyCollection<InputActionReference> ActionRefs { get; }
        public BindingOverrides LocalOverrides { get; protected set; } = new();
        public BindingOverrides GlobalOverrides { get; protected set; } = new();

        private readonly List<RemappableKey> _keys = [];

        private bool _dirty;
        
        internal abstract BindingOverrides GetCurrentBindingOverrides();
        
        internal abstract BindingOverrides GetBindingOverrides(BindingOverrideType overrideType);
        
        protected abstract string GetBindingOverridesPath(BindingOverrideType overrideType);

        public void AddKeys(params RemappableKey?[] keys)
        {
            _keys.AddRange(keys.Where(key => key is not null)!);
        }

        public bool ContainsKey(RemappableKey? key)
        {
            return key is not null && _keys.Contains(key);
        }
        
        public void LoadOverrides(BindingOverrideType overrideType)
        {
            switch (overrideType)
            {
                case BindingOverrideType.Global:
                    if (CurrentType != overrideType && _dirty)
                        LocalOverrides = GetCurrentBindingOverrides();
                    
                    Asset.RemoveAllBindingOverrides();
                    GlobalOverrides.LoadInto(ActionRefs);
                    break;
                case BindingOverrideType.Local:
                    if (CurrentType != overrideType && _dirty)
                        GlobalOverrides = GetCurrentBindingOverrides();
                    
                    Asset.RemoveAllBindingOverrides();
                    LocalOverrides.LoadInto(ActionRefs);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overrideType), overrideType, null);
            }

            CurrentType = overrideType;
            _dirty = true;
        }
        
        public virtual void SaveOverrides()
        {
            switch (CurrentType)
            {
                case BindingOverrideType.Global:
                    GlobalOverrides = GetCurrentBindingOverrides();
                    break;
                case BindingOverrideType.Local:
                    LocalOverrides = GetCurrentBindingOverrides();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            WriteOverridesToDisk();

            _dirty = false;
        }

        protected abstract void WriteOverridesToDisk();
        
        public void DiscardOverrides()
        {
            LocalOverrides = GetBindingOverrides(BindingOverrideType.Local);
            GlobalOverrides = GetBindingOverrides(BindingOverrideType.Global);

            _dirty = false;
        }

        public abstract void Reload();
        
        public abstract BaseContextBindingOverride Reset();
    }
    
    private sealed class ModContextBindingOverride : BaseContextBindingOverride
    {
        private readonly LcInputActions _inputActions;

        public ModContextBindingOverride(LcInputActions inputActions)
        {
            _inputActions = inputActions;

            LocalOverrides = GetBindingOverrides(BindingOverrideType.Local);
            GlobalOverrides = GetBindingOverrides(BindingOverrideType.Global);
        }

        protected override InputActionAsset Asset => _inputActions.Asset;

        protected override IReadOnlyCollection<InputActionReference> ActionRefs => _inputActions.ActionRefs;

        internal override BindingOverrides GetCurrentBindingOverrides() => _inputActions.GetCurrentBindingOverrides();

        internal override BindingOverrides GetBindingOverrides(BindingOverrideType overrideType) =>
            _inputActions.GetBindingOverrides(overrideType);

        protected override string GetBindingOverridesPath(BindingOverrideType overrideType) =>
            _inputActions.GetBindingOverridesPath(overrideType);

        protected override void WriteOverridesToDisk()
        {
            File.WriteAllText(GetBindingOverridesPath(BindingOverrideType.Local), LocalOverrides.AsJson());
            File.WriteAllText(GetBindingOverridesPath(BindingOverrideType.Global), GlobalOverrides.AsJson());
        }

        public override void Reload() => _inputActions.Load();

        public override BaseContextBindingOverride Reset() => new ModContextBindingOverride(_inputActions);
    }

    private sealed class VanillaContextBindingOverride : BaseContextBindingOverride
    {
        private readonly VanillaInputActions _inputActions;
        
        public VanillaContextBindingOverride()
        {
            _inputActions = VanillaInputActions.Instance;

            LocalOverrides = GetBindingOverrides(BindingOverrideType.Local);
            GlobalOverrides = GetBindingOverrides(BindingOverrideType.Global);
        }

        protected override InputActionAsset Asset => _inputActions.Asset;
        protected override IReadOnlyCollection<InputActionReference> ActionRefs => _inputActions.ActionRefs;

        internal override BindingOverrides GetCurrentBindingOverrides() => new(Asset.bindings);

        internal override BindingOverrides GetBindingOverrides(BindingOverrideType overrideType) =>
            _inputActions.GetBindingOverrides(overrideType);

        protected override string GetBindingOverridesPath(BindingOverrideType overrideType) =>
            _inputActions.GetBindingOverridesPath(overrideType);

        public override void SaveOverrides()
        {
            base.SaveOverrides();
            
            // Load global into vanilla settings keybinds and save
            Asset.RemoveAllBindingOverrides();
            
            GlobalOverrides.LoadInto(ActionRefs);
            IngamePlayerSettings.Instance.unsavedSettings.keyBindings = Asset.SaveBindingOverridesAsJson();
            IngamePlayerSettings.Instance.settings.keyBindings = IngamePlayerSettings.Instance.unsavedSettings.keyBindings;
            IngamePlayerSettings.Instance.SaveSettingsToPrefs();
            
            Asset.RemoveAllBindingOverrides();
        }

        protected override void WriteOverridesToDisk()
        {
            File.WriteAllText(GetBindingOverridesPath(BindingOverrideType.Local), LocalOverrides.AsJson());
        }

        public override void Reload() => _inputActions.Load();

        public override BaseContextBindingOverride Reset() => new VanillaContextBindingOverride();
    }
}