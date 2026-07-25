using System;
using UnityEngine;

namespace Showdown4.Utils;

/// <summary>
///     A dead-simple poll-based countdown. It is just a deadline (<see cref="Time.time" /> based)
///     plus two callbacks - no coroutines involved, so nothing can ever cancel or "break" it from
///     the outside. The owner calls <see cref="Tick" /> once per frame (the state machine forwards
///     Unity's Update to the active state); <c>onTick</c> fires once per remaining second and
///     <c>onComplete</c> fires exactly once when the countdown reaches zero.
/// </summary>
public class Countdown
{
	private float _endTime = -1f;
	private int _lastTickedSecond = -1;
	private Action _onComplete;
	private Action<int> _onTick;

	// While paused we stash the seconds that were still remaining and clear the deadline,
	// so Tick() no-ops until Resume() restores the deadline from the stashed remainder.
	private float _pausedRemaining = -1f;

	public bool IsRunning => _endTime >= 0f;

	public bool IsPaused => _pausedRemaining >= 0f;

	public void Start(int seconds, Action<int> onTick = null, Action onComplete = null)
	{
		_endTime = Time.time + seconds;
		_lastTickedSecond = -1;
		_onTick = onTick;
		_onComplete = onComplete;
	}

	public void Stop()
	{
		_endTime = -1f;
		_pausedRemaining = -1f;
	}

	// Freezes the countdown at its current remaining time. A no-op when it is not running
	// (or already paused). Resume() continues from exactly where it was frozen.
	public void Pause()
	{
		if (!IsRunning || IsPaused)
		{
			return;
		}

		_pausedRemaining = Mathf.Max(0f, _endTime - Time.time);
		_endTime = -1f;
	}

	// Continues a paused countdown from the remaining time captured by Pause().
	public void Resume()
	{
		if (!IsPaused)
		{
			return;
		}

		_endTime = Time.time + _pausedRemaining;
		_pausedRemaining = -1f;
	}

	// Call once per frame. Cheap no-op while the countdown is not running.
	public void Tick()
	{
		if (!IsRunning)
		{
			return;
		}

		int remaining = Mathf.Max(0, Mathf.CeilToInt(_endTime - Time.time));
		if (remaining != _lastTickedSecond)
		{
			_lastTickedSecond = remaining;
			_onTick?.Invoke(remaining);
		}

		if (remaining <= 0)
		{
			Stop();
			_onComplete?.Invoke();
		}
	}
}