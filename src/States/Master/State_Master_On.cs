using System;
using Showdown4.Commands;
using Showdown4.States.Showdown;
using Showdown4.Utils;
using ZeepSDK.Chat;

namespace Showdown4.States.Master;

public class State_Master_On : IState
{
    public State_Master_On(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }


    private ShowdownStateMachine _showdownStateMachine => SubStateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }
    public IStateMachine SubStateMachine { get; set; }
    public event Action Finished;

    public void Enter()
    {
        CommandShowdownStart.CommandInvoked += OnShowdownStarted;
        CommandShowdownStop.CommandInvoked += OnShowdownStopped;
        CommandFinishState.CommandInvoked += OnFinishedState;
    }

    public void Execute()
    {
        SubStateMachine = new ShowdownStateMachine();
    }

    public void Exit()
    {
        ChatApi.SendMessage("/joinmessage disable");
        ChatApi.SendMessage("/servermessage remove");
        CommandShowdownStart.CommandInvoked -= OnShowdownStarted;
        CommandShowdownStop.CommandInvoked -= OnShowdownStopped;
        CommandFinishState.CommandInvoked -= OnFinishedState;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void OnFinishedState(string arg)
    {
        SubStateMachine.CurrentState?.InvokeFinish();
    }

    private void OnShowdownStarted()
    {
        ToastMessenger.LogWarning("already running");
    }

    private void OnShowdownStopped()
    {
        Finished?.Invoke();
        ToastMessenger.LogSuccess("Season 5 stopped");
    }
}