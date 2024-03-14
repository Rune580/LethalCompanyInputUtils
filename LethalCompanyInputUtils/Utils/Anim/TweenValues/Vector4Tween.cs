using UnityEngine;
using UnityEngine.Events;

namespace LethalCompanyInputUtils.Utils.Anim.TweenValues;

public struct Vector4Tween : ITweenValue
{
    public float Duration { get; set; }
    public Vector4 StartValue { get; set; }
    public Vector4 TargetValue { get; set; }
    public bool IgnoreTimeScale { get; set; }
    
    private Vector4TweenCallback? _target;
    
    public void TweenValue(float percentage)
    {
        if (!ValidTarget())
            return;

        var newValue = Vector4.Lerp(StartValue, TargetValue, percentage);
        _target!.Invoke(newValue);
    }

    public void AddOnChangedCallback(UnityAction<Vector4> callback)
    {
        _target ??= new Vector4TweenCallback();
        _target.AddListener(callback);
    }

    public bool ValidTarget()
    {
        return _target is not null;
    }

    private class Vector4TweenCallback : UnityEvent<Vector4>;
}