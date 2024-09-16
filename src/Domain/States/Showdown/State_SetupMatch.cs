using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Domain.Entities;
using Showdown4.Utils;
using UnityEngine;
using ZeepSDK.Chat;
using Match = Showdown4.Tmp.Match;
using Team = Showdown4.Tmp.Team;

namespace Showdown4.Domain.States.Showdown;

public class State_SetupMatch : IState
{
    private const int CountdownDuration = 86400;
    private const float TickInterval = 5f;

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
        InitializeTeams();
        RegisterCommands();
        SetCountdownTimer(CountdownDuration);
        StartMatchSetupTimer();
    }

    public void Execute()
    {
        // No periodic logic is required anymore, it's handled by the coroutine.
    }

    public void Exit()
    {
        StopMatchSetupTimer();
        UnregisterCommands();
    }

    private void InitializeTeams()
    {
        _teams = ModStorage.Storage.LoadFromJson<TeamJsonWrapper>("Teams").Teams;
    }

    private void RegisterCommands()
    {
        CommandSetupMatch.CommandInvoked += OnSetTeam;
    }

    private void UnregisterCommands()
    {
        CommandSetupMatch.CommandInvoked -= OnSetTeam;
    }

    private void SetCountdownTimer(int duration)
    {
        ChatApi.SendMessage($"/settime {duration}");
    }

    private void StartMatchSetupTimer()
    {
        if (!_isTimerRunning)
        {
            _isTimerRunning = true;
            CoroutineStarter.Instance.StartExternalCoroutine(CountdownTimerCoroutine(TickInterval));
        }
    }

    private void StopMatchSetupTimer()
    {
        _isTimerRunning = false;
        CoroutineStarter.Instance.StopExternalCoroutine();
    }

    private IEnumerator CountdownTimerCoroutine(float tickInterval)
    {
        // Display the waiting message
        DisplayWaitingMessage();

        // Wait for 2 seconds before setting the teams
        yield return new WaitForSeconds(2f);

        // Simulate the periodic setting of teams after the delay
        OnSetTeam("KBW,PMP");

        // Stop the timer to prevent further ticks (if desired)
        StopMatchSetupTimer();
    }

    private void DisplayWaitingMessage()
    {
        new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Waiting for Host to setup the Match"))
            .AddSeparator()
            .Send();
    }

    private void OnSetTeam(string arg)
    {
        if (TryGetTeamsFromInput(arg, out Team teamA, out Team teamB))
        {
            ShowdownStateMachine.Match = new Match(teamA, teamB);
            Finished?.Invoke();
        }
    }

    private bool TryGetTeamsFromInput(string input, out Team teamA, out Team teamB)
    {
        teamA = null;
        teamB = null;

        string[] teamTags = input.Trim().Split(",");
        if (teamTags.Length != 2)
        {
            ChatApi.AddLocalMessage("Please provide exactly two team tags.");
            return false;
        }

        teamA = _teams.FirstOrDefault(team => team.Tag.Equals(teamTags[0].Trim(), StringComparison.OrdinalIgnoreCase));
        teamB = _teams.FirstOrDefault(team => team.Tag.Equals(teamTags[1].Trim(), StringComparison.OrdinalIgnoreCase));

        if (teamA == null || teamB == null)
        {
            ChatApi.AddLocalMessage("One or both team tags are invalid.");
            return false;
        }

        return true;
    }
}