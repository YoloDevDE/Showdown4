using System;
using Showdown4.Commands;
using Showdown4.States.Showdown;
using Showdown4.Utils;

namespace Showdown4.States.Master;

public class State_Master_On : IState
{
    public State_Master_On(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        SubStateMachine = new ShowdownStateMachine();
    }


    private ShowdownStateMachine _showdownStateMachine => SubStateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }
    public IStateMachine SubStateMachine { get; set; }
    public event Action Finished;

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
        ToastMessenger.LogWarning("already running");
    }

    private void OnShowdownStopped()
    {
        Finished?.Invoke();
        ToastMessenger.LogSuccess("Season 4 stopped");
    }
}