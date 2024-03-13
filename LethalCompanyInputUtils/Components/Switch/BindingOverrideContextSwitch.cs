using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Utils.Anim;
using LethalCompanyInputUtils.Utils.Anim.TweenValues;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LethalCompanyInputUtils.Components.Switch;

public class BindingOverrideContextSwitch : MonoBehaviour
{
    private readonly TweenRunner<Vector2Tween> _maskTweenRunner = new();
    private readonly TweenRunner<Vector2Tween> _selectorTweenRunner = new();
    
    public readonly BindingOverrideContextEvent onBindingOverrideContextChanged = new();
    
    public SwitchMaskGraphic? selectorMask;
    public RectTransform? selector;
    
    private BindingOverrideType _currentType = BindingOverrideType.Global;

    protected BindingOverrideContextSwitch()
    {
        _maskTweenRunner.Init(this);
        _selectorTweenRunner.Init(this);
    }

    public void SwitchToGlobal()
    {
        if (_currentType == BindingOverrideType.Global)
            return;

        _currentType = BindingOverrideType.Global;
        
        MoveSwitch(new Vector2(-45, 0), 0.1f);
        
        onBindingOverrideContextChanged.Invoke(_currentType);
    }

    public void SwitchToLocal()
    {
        if (_currentType == BindingOverrideType.Local)
            return;

        _currentType = BindingOverrideType.Local;
        
        MoveSwitch(new Vector2(45, 0), 0.1f);
        
        onBindingOverrideContextChanged.Invoke(_currentType);
    }

    private void MoveSwitch(Vector2 pos, float duration)
    {
        if (selectorMask is null || selector is null)
            return;
        
        var maskTween = new Vector2Tween
        {
            Duration = duration,
            StartValue = selectorMask.Offset,
            TargetValue = pos,
            IgnoreTimeScale = true
        };
        maskTween.AddOnChangedCallback(MaskTweenCallback);

        var selectorTween = new Vector2Tween
        {
            Duration = duration,
            StartValue = selector.anchoredPosition,
            TargetValue = pos,
            IgnoreTimeScale = true
        };
        selectorTween.AddOnChangedCallback(SelectorTweenCallback);
        
        _maskTweenRunner.StartTween(maskTween);
        _selectorTweenRunner.StartTween(selectorTween);
    }

    private void MaskTweenCallback(Vector2 padding)
    {
        if (selectorMask is null)
            return;

        selectorMask.Offset = padding;
    }
    
    private void SelectorTweenCallback(Vector2 pos)
    {
        if (selector is null)
            return;

        selector.anchoredPosition = pos;
    }

    public class BindingOverrideContextEvent : UnityEvent<BindingOverrideType>;
}