using Showdown4.Commands;
using Showdown4.Service;
using UnityEngine;

namespace Showdown4.Domain.States;

public class State_Master_On : IState
{
    public State_Master_On(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        SubStateMachine = new ShowdownStateMachine();
    }

    public IStateMachine StateMachine { get; }
    public IStateMachine SubStateMachine { get; set; }

    public void Enter()
    {
        CommandShowdownStart.CommandInvoked += OnShowdownStarted;
        CommandShowdownStop.CommandInvoked += OnShowdownStopped;
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        CommandShowdownStart.CommandInvoked -= OnShowdownStarted;
        CommandShowdownStop.CommandInvoked -= OnShowdownStopped;
    }

    private void OnShowdownStarted()
    {
        Messenger.LogWarning("already running");
    }

    private void OnShowdownStopped()
    {
        Messenger.LogCustomColors("stopped", Color.white, Color.magenta);
        StateMachine.TransitionTo(new State_Master_Off(StateMachine));
    }
}