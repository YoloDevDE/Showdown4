using Showdown4.Commands;
using Showdown4.Service;
using Color = UnityEngine.Color;

namespace Showdown4.Domain.States;

public class State_Master_Off : IState
{
    public State_Master_Off(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }

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

    private void OnShowdownStopped()
    {
        Messenger.LogWarning("already stopped");
    }

    private void OnShowdownStarted()
    {
        Messenger.LogCustomColors("started", Color.white, Color.magenta);
        StateMachine.TransitionTo(new State_Master_On(StateMachine));
    }
}