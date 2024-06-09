using System;
using System.Collections.Generic;

namespace Showdown4.Domain.States;

public abstract class State : IState
{
    private readonly List<ITransitionRule> transitionRules = new List<ITransitionRule>();
    public event Action OnCompleted;

    public abstract void Enter(IStateMachine context);
    public abstract void Exit();

    public void CheckTransitions()
    {
        foreach (ITransitionRule rule in transitionRules)
        {
            if (rule.ShouldTransition())
            {
                OnCompleted?.Invoke();
                return;
            }
        }
    }

    public IState GetNextState()
    {
        foreach (ITransitionRule rule in transitionRules)
        {
            if (rule.ShouldTransition())
            {
                return rule.GetNextState();
            }
        }

        return this; // Bleibe im aktuellen Zustand, wenn keine Regel zutrifft
    }

    public void AddTransitionRule(ITransitionRule rule)
    {
        transitionRules.Add(rule);
    }
}