using System;

namespace Showdown4.States;

public interface IState
{
    public IStateMachine StateMachine { get; }

    public IStateMachine SubStateMachine => null;

    event Action Finished;
    void Enter();
    void Execute();
    void Exit();
}