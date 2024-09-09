using System;
using System.Collections.Generic;

namespace Showdown4.Domain.States;

public class MasterStateMachine : IStateMachine
{
    public MasterStateMachine()
    {
        FinalState = new State_Master_Off(this);
        InitialState = new State_Master_Off(this);
        Transitions = new Dictionary<IState, IState>();
    }

    public IState CurrentState { get; set; }
    public IState InitialState { get; }
    public IState FinalState { get; }
    public Dictionary<IState, IState> Transitions { get; }
    public event Action StateMachineFinished;

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}