using System;
using System.Collections;
using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

public class State_SelectInitiative : IState
{
    private const int CountdownDuration = 5; // Countdown in seconds
    private int _currentSelectionIndex; // To track the currently selected team
    private Team _selectedTeam; // The team with initiative
    private List<Team> _teams;

    public State_SelectInitiative(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => (ShowdownStateMachine)StateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        // Initialize the teams and reset selection
        _teams = new List<Team> { _Showdown.Match.TeamA, _Showdown.Match.TeamB };
        _currentSelectionIndex = 0;
        _selectedTeam = null;

        // Register this state for input detection
        StateManager.Instance.SetCurrentState(this);
        Execute(); // Initial display of team selection
    }

    public void Execute()
    {
        // Display the current team selection in the server message
        ServerMessage serverMessage = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Select a team to have the initiative. Use arrow keys to select, and press Space to confirm."));

        for (int index = 0; index < _teams.Count; index++)
        {
            Team team = _teams[index];

            if (index == _currentSelectionIndex)
            {
                // Highlight the currently selected team
                serverMessage.AddLine(line => line.AddBlock($"> {team.GetColoredTagAndName()}", block => block.Bold().Color("#ffff00")));
            }
            else
            {
                // Show the other team normally
                serverMessage.AddLine(line => line.AddBlock($"{team.GetColoredTagAndName()}"));
            }
        }

        serverMessage.AddSeparator().Send();
    }

    public void Exit()
    {
        // Unregister this state from input detection
        StateManager.Instance.SetCurrentState(null);
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    public void HandleInput()
    {
        bool inputDetected = false;

        // Detect arrow key presses
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Toggle between two teams using the arrow keys
            _currentSelectionIndex = (_currentSelectionIndex + 1) % _teams.Count;
            inputDetected = true;
        }

        // Confirm selection with Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectTeamWithInitiative(); // Confirm the selection
            inputDetected = true;
        }

        // Update the display only if there was input
        if (inputDetected)
        {
            Execute();
        }
    }

    private void SelectTeamWithInitiative()
    {
        // Confirm the selected team for initiative
        _selectedTeam = _teams[_currentSelectionIndex];
        ((ShowdownStateMachine)StateMachine).Match.Initiative = _selectedTeam;

        // Start a 5-second countdown and append it to the existing server message
        CoroutineManager.Instance.StartExternalCoroutine(CountdownCoroutine(CountdownDuration));
    }

    private IEnumerator CountdownCoroutine(int countdownTime)
    {
        while (countdownTime > 0)
        {
            // Send or append the countdown message to the server
            ServerMessage msg = new ServerMessage()
                .ShowdownHeader()
                .AddLine(line => line.AddBlock($"Initiative has been given to {_selectedTeam.GetColoredTagAndName()}"))
                .AddSeparator()
                .AddLine(line => line
                    .AddBlock("Continue to")
                    .AddBlock("'Draftphase I'", block => block.Color("#ffff00"))
                    .AddBlock("in")
                    .AddBlock($"{countdownTime}", block => block.Color("#00ff00"))
                    .AddBlock("seconds...")
                )
                .AddSeparator();

            msg.Send();

            yield return new WaitForSeconds(1);
            countdownTime--;
        }

        // After countdown, finish the state
        Finished?.Invoke();
    }
}