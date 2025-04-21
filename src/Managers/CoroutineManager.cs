using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Showdown4.Managers;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance;
    private readonly List<IEnumerator> _activeCoroutines = new List<IEnumerator>();

    public static CoroutineManager Instance
    {
        get
        {
            if (_instance)
            {
                return _instance;
            }

            _instance = Plugin.Instance.gameObject.AddComponent<CoroutineManager>();
            return _instance;
        }
    }

    public static IEnumerator AddCoroutine(IEnumerator coroutine)
    {
        return Instance.StartAndTrackCoroutine(coroutine);
    }

    public static void RemoveCoroutine(IEnumerator coroutine)
    {
        Instance.StopAndRemoveCoroutine(coroutine);
    }

    private IEnumerator StartAndTrackCoroutine(IEnumerator coroutine)
    {
        IEnumerator newCoroutine = RunCoroutine(coroutine);
        _activeCoroutines.Add(coroutine);
        StartCoroutine(newCoroutine);
        return coroutine;
    }

    private void StopAndRemoveCoroutine(IEnumerator coroutine)
    {
        if (_activeCoroutines.Contains(coroutine))
        {
            StopCoroutine(coroutine);
            _activeCoroutines.Remove(coroutine);
        }
    }

    private IEnumerator RunCoroutine(IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);
        RemoveFinishedCoroutine(coroutine);
    }

    private void RemoveFinishedCoroutine(IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            throw new ArgumentNullException("Coroutine must not be null");
        }

        if (_activeCoroutines.Contains(coroutine))
        {
            _activeCoroutines.Remove(coroutine);
        }
    }

    public new static void StopAllCoroutines()
    {
        if (Instance._activeCoroutines == null)
        {
            return;
        }

        foreach (IEnumerator coroutine in Instance._activeCoroutines)
        {
            if (coroutine == null)
            {
                continue;
            }

            Instance.StopCoroutine(coroutine);
        }

        Instance._activeCoroutines.Clear();
    }
}