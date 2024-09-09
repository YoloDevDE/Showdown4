using System;

namespace Showdown4.Domain.States;

public class State_Showdown_Starting : IState
{
    public State_Showdown_Starting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
    }

    public void Execute()
    {
        Finished?.Invoke();
    }

    public void Exit()
    {
    }
}