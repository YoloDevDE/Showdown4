using System;
using System.Collections.Generic;
using Showdown4.Commands;
using Showdown4.Domain.States.Showdown;
using Showdown4.Service;
using UnityEngine;

namespace Showdown4.Domain.States;

public class State_Master_On : IState
{
    public State_Master_On(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        SubStateMachine = new ShowdownStateMachine();

        // Define your states in a list
        List<IState> states = new List<IState>
        {
            SubStateMachine.InitialState,
            new State_SetupMatch(SubStateMachine),
            new State_LinkRacersToTeams(SubStateMachine),
            new State_Drafting(SubStateMachine),
            new State_PreRacing(SubStateMachine)
        };

        // Automatically add transitions between consecutive states


        for (int i = 0; i < states.Count - 1; i++)
        {
            SubStateMachine.AddTransition(states[i], states[i + 1]);
        }
    }


    private ShowdownStateMachine _showdownStateMachine => SubStateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }
    public IStateMachine SubStateMachine { get; set; }
    public event Action Finished;

    public void Enter()
    {
        CommandShowdownStart.CommandInvoked += OnShowdownStarted;
        CommandShowdownStop.CommandInvoked += OnShowdownStopped;
        _showdownStateMachine.ShowdownTimer.Start();
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        _showdownStateMachine.ShowdownTimer.Stop();
        CommandShowdownStart.CommandInvoked -= OnShowdownStarted;
        CommandShowdownStop.CommandInvoked -= OnShowdownStopped;
    }

    private void OnShowdownStarted()
    {
        Messenger.LogWarning("already running");
    }

    private void OnShowdownStopped()
    {
        Finished?.Invoke();
        Messenger.Log("stopped", Color.white, Color.magenta);
    }
}