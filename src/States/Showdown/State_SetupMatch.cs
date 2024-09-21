using System;
using System.Collections;
using System.Collections.Generic;
using Showdown4.Commands;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepSDK.Chat;
using Match = Showdown4.Entities.Match;
using Team = Showdown4.Entities.Team;

namespace Showdown4.States.Showdown;

public class State_SetupMatch : IState
{
    private bool _isTimerRunning;
    private List<Team> _teams;

    public State_SetupMatch(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine ShowdownStateMachine => (ShowdownStateMachine)StateMachine;
    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        // Initialize teams by directly loading from JSON
        // _teams = ModStorage.Storage.LoadFromJson<TeamJsonWrapper>("Teams").Teams;

        // Register command handler
        CommandSetupMatch.CommandInvoked += OnSetTeam;

        // Set the countdown timer
    }

    public void Execute()
    {
        ChatApi.SendMessage("/settime 86400");
        CoroutineManager.Instance.StartExternalCoroutine(CountdownTimerCoroutine());
        // No periodic logic is required anymore, it's handled by the coroutine.
    }

    public void Exit()
    {
        CommandSetupMatch.CommandInvoked -= OnSetTeam;
    }

    private IEnumerator CountdownTimerCoroutine()
    {
        // Display the waiting message directly within the coroutine
        new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Waiting for Host to setup the Match"))
            .AddSeparator()
            .Send();

        // Wait for 2 seconds before setting the teams
        yield return new WaitForSeconds(2f);

        // Simulate the periodic setting of teams after the delay
        OnSetTeam("KBW,PMP");

        // Stop the timer after handling the team setup
        _isTimerRunning = false;
    }

    private void OnSetTeam(string arg)
    {
        Team teamA = null;
        Team teamB = null;

        // Parse team tags and assign the teams directly here
        string[] teamTags = arg.Trim().Split(",");
        if (teamTags.Length != 2)
        {
            ChatApi.AddLocalMessage("Please provide exactly two team tags.");
            return;
        }
        //
        // teamA = _teams.FirstOrDefault(team => team.Tag.Equals(teamTags[0].Trim(), StringComparison.OrdinalIgnoreCase));
        // teamB = _teams.FirstOrDefault(team => team.Tag.Equals(teamTags[1].Trim(), StringComparison.OrdinalIgnoreCase));
        //
        // if (teamA == null || teamB == null)
        // {
        //     ChatApi.AddLocalMessage("One or both team tags are invalid.");
        //     return;
        // }

        teamA = new Team("cool mega nerds", "Nerd", "#ff00ff");
        teamB = new Team("red and fast", "Maki", "#ff0000");
        ShowdownStateMachine.Match = new Match(teamA, teamB);
        Finished?.Invoke();
    }
}