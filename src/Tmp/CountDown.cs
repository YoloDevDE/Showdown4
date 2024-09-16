using System;
using System.Collections;
using UnityEngine;

namespace Showdown4.Tmp;

public class CountDown : MonoBehaviour
{
    public static IEnumerator Seconds(float seconds, Action<float> updateCallback = null, Action callback = null)
    {
        while (seconds > 0)
        {
            // Call the updateCallback to provide the remaining time
            updateCallback?.Invoke(seconds);

            yield return new WaitForSeconds(1);
            seconds--;
        }

        // If a callback is provided, invoke it after the countdown ends
        callback?.Invoke();
    }
}