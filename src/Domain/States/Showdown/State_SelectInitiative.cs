using System;

namespace Showdown4.Domain.States.Showdown;

public class State_SelectInitiative : IState
{
    public State_SelectInitiative(IStateMachine stateMachine)
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
    }

    public void Exit()
    {
    }
}