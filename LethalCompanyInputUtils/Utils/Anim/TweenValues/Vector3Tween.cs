using UnityEngine;
using UnityEngine.Events;

namespace LethalCompanyInputUtils.Utils.Anim.TweenValues;

public struct Vector3Tween : ITweenValue
{
    public float Duration { get; set; }
    public Vector3 StartValue { get; set; }
    public Vector3 TargetValue { get; set; }
    public bool IgnoreTimeScale { get; set; }
    
    private Vector3TweenCallback? _target;
    
    public void TweenValue(float percentage)
    {
        if (!ValidTarget())
            return;

        var newValue = Vector3.Lerp(StartValue, TargetValue, percentage);
        _target!.Invoke(newValue);
    }

    public void AddOnChangedCallback(UnityAction<Vector3> callback)
    {
        _target ??= new Vector3TweenCallback();
        _target.AddListener(callback);
    }

    public bool ValidTarget()
    {
        return _target is not null;
    }

    private class Vector3TweenCallback : UnityEvent<Vector3>;
}
