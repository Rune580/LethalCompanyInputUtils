using LethalCompanyInputUtils.Config;
using LethalCompanyInputUtils.Utils.Anim;
using LethalCompanyInputUtils.Utils.Anim.TweenValues;
using UnityEngine;
using UnityEngine.Events;

namespace LethalCompanyInputUtils.Components.Switch;

public class BindingOverrideContextSwitch : MonoBehaviour
{
    public static BindingOverrideType currentContext = BindingOverrideType.Global;
    
    private readonly TweenRunner<Vector2Tween> _maskTweenRunner = new();
    private readonly TweenRunner<Vector2Tween> _selectorTweenRunner = new();
    
    public readonly BindingOverrideContextEvent onBindingOverrideContextChanged = new();
    
    public SwitchMaskGraphic? selectorMask;
    public RectTransform? selector;
    
    protected BindingOverrideContextSwitch()
    {
        _maskTweenRunner.Init(this);
        _selectorTweenRunner.Init(this);
    }

    public void SwitchToGlobal()
    {
        if (currentContext == BindingOverrideType.Global)
            return;

        currentContext = BindingOverrideType.Global;
        
        MoveSwitch(new Vector2(-45, 0), 0.1f);
        
        onBindingOverrideContextChanged.Invoke(currentContext);
    }

    public void SwitchToLocal()
    {
        if (currentContext == BindingOverrideType.Local)
            return;

        currentContext = BindingOverrideType.Local;
        
        MoveSwitch(new Vector2(45, 0), 0.1f);
        
        onBindingOverrideContextChanged.Invoke(currentContext);
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