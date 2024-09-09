using System;
using UnityEngine;

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
        Debug.Log("IM HERE HELLOOOOO");
        Finished?.Invoke();
        Debug.Log("I INVOKED BUT NOTHING HAPPEN LOLOLOL");
    }

    public void Exit()
    {
    }
}