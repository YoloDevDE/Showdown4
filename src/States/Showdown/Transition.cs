using System;

namespace Showdown4.States.Showdown;

public class Transition : ITransition
{
    private readonly Func<bool> _condition; // Condition function

    private Transition(IState from, IState to, Func<bool> condition)
    {
        From = from;
        To = to;
        _condition = condition;
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

    public static Transition CreateInstance(IState from, IState to, Func<bool> condition = null)
    {
        // If the condition is null, default it to always return true
        return new Transition(from, to, condition ?? (() => true));
    }
}