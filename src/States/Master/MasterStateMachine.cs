using System;
using System.Collections.Generic;
using UnityEngine;

namespace Showdown4.States.Master;

public class MasterStateMachine : MonoBehaviour, IStateMachine
{
    public MasterStateMachine()
    {
        Transitions = new List<ITransition>();
    }

    public IState CurrentState { get; set; }

    // Instead of assigning a value in the constructor, use properties that return new instances
    public IState InitialState => new State_Master_Off(this);
    public IState FinalState => new State_Master_Off(this);

    public List<ITransition> Transitions { get; set; }
    public event Action StateMachineFinished;

    public void InitTransitions()
    {
        IStateMachine stateMachine = this;
        Transitions = new List<ITransition>();
        // Add transitions with new instances directly
        stateMachine
            .AddTransition(new State_Master_Off(this), new State_Master_On(this))
            .AddTransition(new State_Master_On(this), new State_Master_Off(this));
    }

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}