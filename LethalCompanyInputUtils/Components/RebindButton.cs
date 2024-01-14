using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.Glyphs;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components;

public class RebindButton : MonoBehaviour
{
    public Selectable? button;
    public TextMeshProUGUI? bindLabel;
    public Image? glyphLabel;
    public Image? notSupportedImage;
    public RebindIndicator? rebindIndicator;
    public Button? resetButton;
    public Button? removeButton;
    public float timeout = 5f;
    
    private RemappableKey? _key;
    private bool _isBaseGame;
    private InputActionRebindingExtensions.RebindingOperation? _rebindingOperation;
    private bool _rebinding;
    private float _timeoutTimer;
    
    private static MethodInfo? _setChangesNotAppliedMethodInfo;
    private static readonly List<RebindButton> Instances = [];

    public void SetKey(RemappableKey? key, bool isBaseGame)
    {
        _key = key;
        _isBaseGame = isBaseGame;
        
        UpdateState();
    }

    public void UpdateState()
    {
        if (bindLabel is null || glyphLabel is null || button is null || notSupportedImage is null || resetButton is null || removeButton is null)
            return;

        if (_key is null)
        {
            SetAsUnsupported();
            return;
        }

        var bindingIndex = GetRebindingIndex();
        var action = _key.currentInput.action;

        if (bindingIndex >= action.bindings.Count)
        {
            SetAsUnsupported();
            return;
        }

        resetButton.gameObject.SetActive(action.bindings[bindingIndex].hasOverrides);

        var effectivePath = action.bindings[bindingIndex].effectivePath;
        
        var bindPath = InputControlPath.ToHumanReadableString(effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

        if (_key.gamepadOnly)
        {
            bindLabel.SetText("");

            if (effectivePath == LcInputActions.UnboundGamepadIdentifier)
            {
                removeButton.gameObject.SetActive(false);
                glyphLabel.enabled = false;
                return;
            }
            
            removeButton.gameObject.SetActive(true);

            var glyphSet = ControllerGlyph.GetBestMatching();
            if (glyphSet is null)
            {
                bindLabel.SetText(effectivePath);
                glyphLabel.enabled = false;
                return;
            }
            
            var glyph = glyphSet[effectivePath];

            if (glyph is null)
            {
                bindLabel.SetText(effectivePath);
                glyphLabel.enabled = false;
                return;
            }

            glyphLabel.sprite = glyph;
            glyphLabel.enabled = true;
        }
        else
        {
            removeButton.gameObject.SetActive(!string.Equals(effectivePath, LcInputActions.UnboundKeyboardAndMouseIdentifier));
            
            glyphLabel.enabled = false;
            bindLabel.SetText(bindPath);
        }
    }

    private void SetAsUnsupported()
    {
        if (button is null || bindLabel is null || glyphLabel is null || notSupportedImage is null || resetButton is null || removeButton is null)
            return;
        
        button.interactable = false;
        button.targetGraphic.raycastTarget = false;
        button.targetGraphic.enabled = false;
        bindLabel.SetText("");
        glyphLabel.enabled = false;
        notSupportedImage.gameObject.SetActive(true);
            
        resetButton.gameObject.SetActive(false);
        removeButton.gameObject.SetActive(false);
    }

    private int GetRebindingIndex()
    {
        if (_key is null)
            return -1;

        var action = _key.currentInput.action;

        if (action.controls.Count == 0)
        {
            if (action.bindings.Count == 0)
                return -1;

            if (_key.gamepadOnly)
                return 1;
            
            return 0;
        }

        return _key.rebindingIndex < 0 ? action.GetBindingIndexForControl(action.controls[0]) : _key.rebindingIndex;
    }

    public void StartRebinding()
    {
        if (_key is null || bindLabel is null || glyphLabel is null || rebindIndicator is null || resetButton is null || removeButton is null)
            return;

        var rebindIndex = GetRebindingIndex();

        resetButton.interactable = false;
        removeButton.interactable = false;

        if (_key.gamepadOnly)
        {
            glyphLabel.enabled = false;
            rebindIndicator.enabled = true;
            
            RebindGamepad(_key.currentInput, rebindIndex);
        }
        else
        {
            bindLabel.SetText("");
            rebindIndicator.enabled = true;
            
            RebindKbm(_key.currentInput, rebindIndex);
        }
        
        _timeoutTimer = timeout;
        _rebinding = true;
    }

    private void FinishRebinding()
    {
        if (_key is null || rebindIndicator is null || resetButton is null || removeButton is null)
            return;
        
        rebindIndicator.enabled = false;
        _rebinding = false;

        if (_rebindingOperation is not null)
            _rebindingOperation = null;
        
        resetButton.interactable = true;
        removeButton.interactable = true;
        
        UpdateState();
    }
    
    public void ResetToDefault()
    {
        FinishRebinding();

        if (_key is null)
            return;

        var bindIndex = GetRebindingIndex();
        var action = _key.currentInput.action;
        
        if (!action.bindings[bindIndex].hasOverrides)
            return;
        
        action.RemoveBindingOverride(bindIndex);

        if (_isBaseGame)
            BaseGameUnsavedChanges();
        
        MarkSettingsAsDirty();
        UpdateState();
    }

    public void RemoveBind()
    {
        FinishRebinding();

        if (_key is null)
            return;

        var bindIndex = GetRebindingIndex();
        var action = _key.currentInput.action;

        action.ApplyBindingOverride(bindIndex,
            _key.gamepadOnly
                ? LcInputActions.UnboundGamepadIdentifier
                : LcInputActions.UnboundKeyboardAndMouseIdentifier);

        if (_isBaseGame)
            BaseGameUnsavedChanges();

        MarkSettingsAsDirty();
        UpdateState();
    }
    
    private void OnEnable()
    {
        Instances.Add(this);

        if (_key is null)
            return;
        
        UpdateState();
    }

    private void OnDisable()
    {
        FinishRebinding();
        
        Instances.Remove(this);
    }

    private void Update()
    {
        if (!_rebinding)
            return;
        
        _timeoutTimer -= Time.deltaTime;
        if (_timeoutTimer > 0f)
            return;

        if (_rebindingOperation is null)
        {
            FinishRebinding();
            return;
        }
        
        _rebindingOperation.Cancel();
        FinishRebinding();
    }

    private void RebindKbm(InputActionReference inputActionRef, int rebindIndex)
    {
        _rebindingOperation = inputActionRef.action.PerformInteractiveRebinding(rebindIndex)
            .OnMatchWaitForAnother(0.1f)
            .WithControlsHavingToMatchPath("<Keyboard>")
            .WithControlsHavingToMatchPath("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation => OnRebindComplete(operation, this))
            .OnCancel(_ => FinishRebinding())
            .Start();
    }

    private void RebindGamepad(InputActionReference inputActionRef, int rebindIndex)
    {
        _rebindingOperation = inputActionRef.action.PerformInteractiveRebinding(rebindIndex)
            .OnMatchWaitForAnother(0.1f)
            .WithControlsHavingToMatchPath("<Gamepad>")
            .OnComplete(operation => OnRebindComplete(operation, this))
            .Start();
    }
    
    private static void OnRebindComplete(InputActionRebindingExtensions.RebindingOperation operation, RebindButton instance)
    {
        if (!operation.completed)
            return;

        if (instance._isBaseGame)
            BaseGameUnsavedChanges();
        
        MarkSettingsAsDirty();
        
        instance.FinishRebinding();
    }

    private static void BaseGameUnsavedChanges()
    {
        IngamePlayerSettings.Instance.unsavedSettings.keyBindings =
            IngamePlayerSettings.Instance.playerInput.actions.SaveBindingOverridesAsJson();
    }

    private static void MarkSettingsAsDirty()
    {
        _setChangesNotAppliedMethodInfo ??= AccessTools.Method(typeof(IngamePlayerSettings), "SetChangesNotAppliedTextVisible");
        _setChangesNotAppliedMethodInfo.Invoke(IngamePlayerSettings.Instance, [true]);
    }
    
    public static void ReloadGlyphs()
    {
        foreach (var rebindButton in Instances)
        {
            rebindButton.UpdateState();
        }
    }

    public static void ResetAllToDefaults()
    {
        foreach (var rebindButton in Instances)
            rebindButton.ResetToDefault();
    }
}