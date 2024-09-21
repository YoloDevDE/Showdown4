using System;
using System.Collections.Generic;
using Showdown4.States.Showdown;

namespace Showdown4.States.Master;

public class MasterStateMachine : IStateMachine
{
    public MasterStateMachine()
    {
        Transitions = [];
        FinalState = new State_Master_Off(this);
        InitialState = new State_Master_Off(this);
    }

    public IState CurrentState { get; set; }
    public IState InitialState { get; }
    public IState FinalState { get; }
    public List<ITransition> Transitions { get; }
    public event Action StateMachineFinished;

    public void InitTransitions()
    {
        IStateMachine stateMachine = this;
        IState stateMasterOn = new State_Master_On(this);
        stateMachine
            .AddTransition(Transition.CreateInstance(InitialState, stateMasterOn))
            .AddTransition(Transition.CreateInstance(stateMasterOn, InitialState));
    }

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}