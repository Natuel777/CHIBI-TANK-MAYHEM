using UnityEngine;
using System.Collections;
using System;

public class IdleBehaviour : IBehaviours
{
    private bool _active, _corroutineStarted;
    private float _searchTimer;
    private Func<IEnumerator, Coroutine> _startCoroutine;

    public IdleBehaviour(Func<IEnumerator, Coroutine> sc, float searchTimer)
    {
        _searchTimer = searchTimer;
        _startCoroutine = sc;
    }

    public void Active(bool value) {_active = value;}

    public void ArtificialUpdate()
    {
        if(!_active || _corroutineStarted) return;

        _startCoroutine(SearchForTarget());
    }

    private IEnumerator SearchForTarget()
    {
        _corroutineStarted = true;
        yield return new WaitForSeconds(_searchTimer);
    }
}
