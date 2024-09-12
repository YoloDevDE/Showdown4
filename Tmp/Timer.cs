using System;
using System.Timers;

namespace Showdown4.Tmp;

public class Timer
{
    private readonly System.Timers.Timer _timer;

    // Constructor allows setting custom interval in milliseconds
    public Timer(double intervalInMilliseconds = 1000) // Default to 1 second if not provided
    {
        _timer = new System.Timers.Timer(intervalInMilliseconds);
        _timer.Elapsed += OnTimedEvent;
        _timer.AutoReset = true;
    }

    public int Ticks { get; private set; }

    public event Action Tick;

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        Ticks++;
        Tick?.Invoke();
    }

    public void Start()
    {
        Ticks = 0;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    // Reset the Ticks and stop the timer
    public void Reset()
    {
        Ticks = 0;
        Stop();
        Start();
    }
}