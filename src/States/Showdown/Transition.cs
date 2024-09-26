using System;

namespace Showdown4.States.Showdown;

public class Transition : ITransition
{
    private readonly Func<bool> _condition; // Condition function

    public Transition(IState from, IState to, Func<bool> condition = null)
    {
        From = from;
        To = to;
        _condition = condition ?? (() => true);
    }

    public IState From { get; }
    public IState To { get; }


    public bool CanTransition()
    {
        return _condition(); // Evaluate the condition
    }

    public void OnTransition()
    {
        // Optional: Add any transition-specific logic here
    }
}