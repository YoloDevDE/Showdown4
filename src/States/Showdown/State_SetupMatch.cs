using System;
using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class State_SetupMatch : IState
{
    private const int CountdownDuration = 5; // Countdown in seconds
    private int _currentSelectionIndex;
    private bool _isCountdownActive;
    private int _remainingSeconds;
    private Team _selectedTeamA;
    private Team _selectedTeamB;
    private List<Team> _teams;

    public State_SetupMatch(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;
    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        TeamData teamData = Plugin.Storage.LoadFromJson<TeamData>(Plugin.TeamFile.Value);
        _teams = teamData?.Teams ?? new List<Team>();

        _currentSelectionIndex = 0;
        _selectedTeamA = null;
        _selectedTeamB = null;
        _isCountdownActive = false;

        ChatApi.SendMessage("/timeset 86400");
        if (_teams.Count == 0)
        {
            ChatApi.SendMessage("No teams found in the JSON file.");
        }

        StateManager.Instance.SetCurrentState(this);
        Execute();
    }

    public void Execute()
    {
        SendServerMessage();
    }

    public void Exit()
    {
        StateManager.Instance.SetCurrentState(null);
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    public void HandleInput()
    {
        bool inputDetected = false;

        if (!_isCountdownActive)
        {
            // Detect arrow key presses
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentSelectionIndex = (_currentSelectionIndex - 1 + _teams.Count) % _teams.Count; // Move up
                inputDetected = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentSelectionIndex = (_currentSelectionIndex + 1) % _teams.Count; // Move down
                inputDetected = true;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectTeam(); // Confirm team selection
                inputDetected = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ResetSelection(); // Reset selection
                inputDetected = true;
            }
        }

        // Start countdown if space is pressed and teams are selected
        if (Input.GetKeyDown(KeyCode.Space) && _selectedTeamA != null && _selectedTeamB != null && !_isCountdownActive)
        {
            ConfirmTeams();
            StartCountdown();
        }

        if (inputDetected)
        {
            Execute();
        }
    }

    private void SendServerMessage()
    {
        // ServerMessage showing the team selection
        ServerMessage serverMessage = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Setup Match"))
            .AddSeparator();

        for (int index = 0; index < _teams.Count; index++)
        {
            Team team = _teams[index];

            if (team == _selectedTeamA || team == _selectedTeamB)
            {
                serverMessage.AddLine(line => line.AddBlock($"{index}: " + team.GetColoredTagAndName(), block => block.Strikethrough()));
            }
            else if (index == _currentSelectionIndex)
            {
                serverMessage.AddLine(line => line.AddBlock($"> {index}: " + team.GetColoredTagAndName(), block => block.Bold().Color("#ffff00")));
            }
            else
            {
                serverMessage.AddLine(line => line.AddBlock($"{index}: " + team.GetColoredTagAndName()));
            }
        }

        // Display the countdown if it's active
        if (_isCountdownActive)
        {
            serverMessage.AddSeparator()
                .AddLine(line => line
                    .AddBlock("Continue to")
                    .AddBlock("'Link Racers'", block => block.Color("#ffff00"))
                    .AddBlock("in")
                    .AddBlock($"{_remainingSeconds}", block => block.Color("#00ff00"))
                    .AddBlock("seconds..."));
        }

        serverMessage.Send();
    }

    private void SelectTeam()
    {
        Team selectedTeam = _teams[_currentSelectionIndex];

        if (_selectedTeamA == null)
        {
            _selectedTeamA = selectedTeam;
        }
        else if (_selectedTeamB == null && selectedTeam != _selectedTeamA)
        {
            _selectedTeamB = selectedTeam;
            ChatApi.SendMessage($"Teams selected: {_selectedTeamA.GetTag()} vs {_selectedTeamB.GetTag()}. Press space to confirm.");
        }
    }

    private void ResetSelection()
    {
        _selectedTeamA = null;
        _selectedTeamB = null;
        ChatApi.SendMessage("Team selections have been reset.");
    }

    private void StartCountdown()
    {
        if (_selectedTeamA != null && _selectedTeamB != null)
        {
            _isCountdownActive = true;
            TimerUtility.StartCountdown(
                CountdownDuration, // Duration of the countdown
                remainingSeconds =>
                {
                    _remainingSeconds = remainingSeconds;
                    Execute(); // Action to update message during countdown
                },
                InvokeFinish // Action when countdown completes
            );
        }
    }

    private void ConfirmTeams()
    {
        ChatApi.SendMessage($"Teams confirmed: {_selectedTeamA.GetTag()} vs {_selectedTeamB.GetTag()}.");
        Showdown.Match = new Match(_selectedTeamA, _selectedTeamB);
    }
}