using System;
using Showdown4.Commands;
using Showdown4.Utils;

namespace Showdown4.States.Master;

public class State_Master_Off : IState
{
    public State_Master_Off(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }

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

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void OnShowdownStopped()
    {
        ToastMessenger.LogWarning("already stopped");
    }

    private void OnShowdownStarted()
    {
        Finished?.Invoke();

        ToastMessenger.LogSuccess("Season 5 started");
    }
}