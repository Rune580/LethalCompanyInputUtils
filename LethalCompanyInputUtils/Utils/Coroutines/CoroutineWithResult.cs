using System.Collections;
using UnityEngine;

namespace LethalCompanyInputUtils.Utils.Coroutines;

// Taken from https://discussions.unity.com/t/how-do-i-return-a-value-from-a-coroutine/6438/4
// With some modifications
internal class CoroutineWithResult : CustomYieldInstruction
{
    private readonly IEnumerator _target;
    private object? _result;
    private bool _canMoveNext;

    public override bool keepWaiting => _canMoveNext;

    public CoroutineWithResult(MonoBehaviour container, IEnumerator target)
    {
        _target = target;

        container.StartCoroutine(Run());
    }

    public Optional<T> GetResult<T>()
    {
        if (_result is not T typedResult)
            return Optional<T>.None();
        
        return Optional<T>.Some(typedResult);
    }

    private IEnumerator Run()
    {
        do
        {
            _canMoveNext = _target.MoveNext();
            _result = _target.Current;
            yield return _result;
        } while (_canMoveNext);
    }
}