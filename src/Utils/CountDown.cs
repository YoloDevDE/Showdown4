using System;
using System.Collections;
using UnityEngine;

namespace Showdown4.Utils;

public class CountdownTimer
{
	// Enumerator method that runs the countdown and triggers actions on tick and completion
	public static IEnumerator Start(int durationInSeconds, Action<int> onTick, Action onComplete)
	{
		var remainingTime = durationInSeconds;

		while (remainingTime >= 0)
		{
			// Invoke the onTick method with the current remaining time
			onTick?.Invoke(remainingTime);

			// Wait for 1 second
			yield return new WaitForSeconds(1);

			// Decrement the remaining time
			remainingTime--;
		}

		// Final invoke once the countdown is complete
		onComplete?.Invoke();
	}
}