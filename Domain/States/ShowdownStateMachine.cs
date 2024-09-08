using System;
using Showdown4.Tmp;

namespace Showdown4.Domain.States;

public class ShowdownStateMachine : IStateMachine
{
    public Match CurrentMatch;

    public ShowdownStateMachine()
    {
        InitialState = new State_SetTeams(this);
        FinalState = new State_Match_End(this);
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