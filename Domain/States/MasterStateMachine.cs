using System;

namespace Showdown4.Domain.States;

public class MasterStateMachine : IStateMachine
{
    public MasterStateMachine()
    {
        FinalState = new State_Master_Off(this);
        InitialState = new State_Master_Off(this);
    }

    public IState CurrentState { get; set; }
    public IState InitialState { get; }
    public IState FinalState { get; }
    public event Action StateMachineFinished;

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}