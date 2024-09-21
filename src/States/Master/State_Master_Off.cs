using System;
using Showdown4.Commands;
using Showdown4.Utils;
using UnityEngine;
using ZeepSDK.Racing;
using Color = UnityEngine.Color;

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
        RacingApi.RoundStarted += OnShowdownStarted;
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        RacingApi.RoundStarted -= OnShowdownStarted;
        CommandShowdownStart.CommandInvoked -= OnShowdownStarted;
        CommandShowdownStop.CommandInvoked -= OnShowdownStopped;
    }

    private void OnShowdownStopped()
    {
        ToastMessenger.LogWarning("already stopped");
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

        ToastMessenger.Log("started", Color.white, Color.magenta);
    }
}