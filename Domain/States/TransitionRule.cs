using System;

namespace Showdown4.Domain.States;

public class TransitionRule : ITransitionRule
{
    private readonly Func<bool> condition;
    private readonly IState nextState;

    public TransitionRule(Func<bool> condition, IState nextState)
    {
        this.condition = condition;
        this.nextState = nextState;
    }

    public bool ShouldTransition()
    {
        return condition();
    }

    public IState GetNextState()
    {
        return nextState;
    }
}