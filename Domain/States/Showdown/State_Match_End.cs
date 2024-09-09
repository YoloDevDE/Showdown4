using System;
using Showdown4.Domain.States.Showdown;

namespace Showdown4.Domain.States;

public class State_Match_End : IState
{
    public State_Match_End(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine ShowdownStateMachine => (ShowdownStateMachine)StateMachine;
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