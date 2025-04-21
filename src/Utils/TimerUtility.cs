using System;
using System.Collections;
using Showdown4.Managers;
using UnityEngine;

namespace Showdown4.Utils;

public class TimerUtility : MonoBehaviour
{
    private static IEnumerator _activeCountdown;

    public static void StartCountdown(int durationInSeconds, Action<int> onTick, Action onComplete, float intervalInSeconds = 1f)
    {
        StopCountdown(); // Ensure only one countdown is running at a time
        _activeCountdown = CoroutineManager.AddCoroutine(CountdownCoroutine(durationInSeconds, onTick, onComplete, intervalInSeconds));
    }

    public static void StopCountdown()
    {
        if (_activeCountdown == null)
        {
            return;
        }

        CoroutineManager.RemoveCoroutine(_activeCountdown);
        _activeCountdown = null;
    }

    private static IEnumerator CountdownCoroutine(int durationInSeconds, Action<int> onTick, Action onComplete, float intervalInSeconds = 1f)
    {
        intervalInSeconds = Math.Abs(intervalInSeconds);
        for (int remainingTime = Math.Abs(durationInSeconds) + 1; remainingTime >= 0; remainingTime--)
        {
            onTick?.Invoke(remainingTime);
            yield return new WaitForSeconds(intervalInSeconds);
        }

        onComplete?.Invoke();
        _activeCountdown = null; // Reset the reference when finished
    }
}