using System;
using System.Collections.Generic;
using Showdown4.Tmp;

namespace Showdown4.Domain.States.Showdown;

public class ShowdownStateMachine : IStateMachine
{
    public Match CurrentMatch;

    public ShowdownStateMachine()
    {
        InitialState = new State_Showdown_Starting(this);
        FinalState = new State_Match_End(this);
        Transitions = new Dictionary<IState, IState>();
        ShowdownTimer = new TimerHelper();
    }

    public TimerHelper ShowdownTimer { get; }

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