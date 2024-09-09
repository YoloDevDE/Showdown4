using System;
using System.Timers;

namespace Showdown4.Domain.States.Showdown;

public class TimerHelper
{
    private readonly Timer _timer;

    public TimerHelper()
    {
        _timer = new Timer(250);
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
}