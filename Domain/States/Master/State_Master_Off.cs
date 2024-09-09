using System;
using Showdown4.Commands;
using Showdown4.Service;
using UnityEngine;
using ZeepSDK.Racing;
using Color = UnityEngine.Color;

namespace Showdown4.Domain.States;

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
        RacingApi.RoundStarted += OnShowdownStarted;
        CommandShowdownStop.CommandInvoked += OnShowdownStopped;
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        CommandShowdownStart.CommandInvoked -= OnShowdownStarted;
        RacingApi.RoundStarted -= OnShowdownStarted;
        CommandShowdownStop.CommandInvoked -= OnShowdownStopped;
    }

    private void OnShowdownStopped()
    {
        Messenger.LogWarning("already stopped");
    }

    private void OnShowdownStarted()
    {
        if (Finished != null)
        {
            Debug.Log("Invoking Finished event");
            Finished.Invoke();
        }
        else
        {
            Debug.LogError("Finished event is null, no subscribers");
        }

        Messenger.Log("started", Color.white, Color.magenta);
    }
}