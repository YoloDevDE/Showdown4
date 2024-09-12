using System;

namespace Showdown4.Tmp;

public class CountdownTimer
{
    private readonly Timer _timer;

    public CountdownTimer(int seconds)
    {
        SecondsLeft = seconds;
        _timer = new Timer(); // 1-second interval
        _timer.Tick += OnTick;
    }

    public int SecondsLeft { get; private set; }

    public event Action CountdownTick; // Event to notify the remaining seconds
    public event Action CountdownFinished; // Event to notify when the countdown reaches zero

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    // Reset the countdown to its original value and stop the timer
    public void Reset(int newSeconds = -1)
    {
        if (newSeconds > 0)
        {
            SecondsLeft = newSeconds;
        }
        else
        {
            SecondsLeft = 0;
        }

        _timer.Reset(); // Reset the internal timer
    }

    private void OnTick()
    {
        SecondsLeft--;

        // Trigger event for each tick
        CountdownTick?.Invoke();

        if (SecondsLeft < 0)
        {
            _timer.Stop();
            CountdownFinished?.Invoke();
        }
    }
}