using UnityEngine;
using UnityEngine.Events;

namespace LethalCompanyInputUtils.Utils.Anim.TweenValues;

public struct Vector2Tween : ITweenValue
{
    public float Duration { get; set; }
    public Vector2 StartValue { get; set; }
    public Vector2 TargetValue { get; set; }
    public bool IgnoreTimeScale { get; set; }
    
    private Vector2TweenCallback? _target;
    
    public void TweenValue(float percentage)
    {
        if (!ValidTarget())
            return;

        var newValue = Vector2.Lerp(StartValue, TargetValue, percentage);
        _target!.Invoke(newValue);
    }

    public void AddOnChangedCallback(UnityAction<Vector2> callback)
    {
        _target ??= new Vector2TweenCallback();
        _target.AddListener(callback);
    }

    public bool ValidTarget()
    {
        return _target is not null;
    }

    private class Vector2TweenCallback : UnityEvent<Vector2>;
}