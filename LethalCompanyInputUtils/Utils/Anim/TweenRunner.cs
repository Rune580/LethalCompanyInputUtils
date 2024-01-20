using System.Collections;
using UnityEngine;

namespace LethalCompanyInputUtils.Utils.Anim;

public class TweenRunner<T> where T : struct, ITweenValue
{
    protected MonoBehaviour? CoroutineContainer;
    protected IEnumerator? Tween;

    private static IEnumerator Start(T tweenValue)
    {
        if (!tweenValue.ValidTarget())
            yield break;

        var elapsedTime = 0f;
        while (elapsedTime < tweenValue.Duration)
        {
            elapsedTime += tweenValue.IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            var percentage = Mathf.Clamp01(elapsedTime / tweenValue.Duration);
            tweenValue.TweenValue(percentage);
            yield return null;
        }

        tweenValue.TweenValue(1f);
    }

    public void Init(MonoBehaviour coroutineContainer)
    {
        CoroutineContainer = coroutineContainer;
    }

    public void StartTween(T tweenValue)
    {
        if (CoroutineContainer is null)
            return;

        if (Tween is not null)
        {
            CoroutineContainer.StopCoroutine(Tween);
            Tween = null;
        }

        if (!CoroutineContainer.gameObject.activeInHierarchy)
        {
            tweenValue.TweenValue(1f);
            return;
        }

        Tween = Start(tweenValue);
        CoroutineContainer.StartCoroutine(Tween);
    }

    public void StopTween()
    {
        if (Tween is null || CoroutineContainer is null)
            return;
        
        CoroutineContainer.StopCoroutine(Tween);
        Tween = null;
    }
}