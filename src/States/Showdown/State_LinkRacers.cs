using System;
using System.Collections;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

public class State_LinkRacers : IState
{
    private const int CountdownDuration = 5;
    private bool _isCountdownRunning;
    private Team _teamA, _teamB, _currentTeam;

    public State_LinkRacers(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        // Initialize variables with default values
        _teamA = _Showdown.Match.TeamA ?? new Team("TeamA", "A", "#ff0000"); // Example of default team
        _teamB = _Showdown.Match.TeamB ?? new Team("TeamB", "B", "#0000ff"); // Example of default team
        _currentTeam = _teamA; // Default to TeamA initially
        _isCountdownRunning = false; // Set countdown running flag to false

        // Ensure there are no racers linked at the start
        _teamA.Racers.Clear();
        _teamB.Racers.Clear();

        CommandLinkRacer.CommandInvoked += OnLinkRacerToTeam;
        CheckIfRacersAreLinked(); // Start linking process
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        CommandLinkRacer.CommandInvoked -= OnLinkRacerToTeam;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void CheckIfRacersAreLinked()
    {
        if (_teamA.Racers.Count >= _teamA.MaxTeamSize && _teamB.Racers.Count >= _teamB.MaxTeamSize)
        {
            StartCountdown();
        }
        else
        {
            _currentTeam = _teamA.Racers.Count < _teamA.MaxTeamSize ? _teamA : _teamB;
            SendLinkingStatusMessage(); // Update message with linking status
        }
    }

    private void StartCountdown()
    {
        if (!_isCountdownRunning)
        {
            _isCountdownRunning = true; // Set flag to avoid multiple starts
            CoroutineManager.Instance.StartExternalCoroutine(
                CountdownCoroutine(CountdownDuration)); // Start the countdown
        }
    }

    private IEnumerator CountdownCoroutine(int countdown)
    {
        while (countdown > 0)
        {
            OnTimerTick(countdown); // Update message every second
            yield return new WaitForSeconds(1); // Wait for 1 second
            countdown--; // Decrease the countdown value
        }

        Finished?.Invoke(); // Notify when countdown finishes
    }

    private void OnTimerTick(int countdownTime)
    {
        // Send or append the countdown message to the server
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddSeparator()
            .AddLine(line => line
                .Bold()
                .AddBlock($"{_Showdown.Match.TeamA.GetColoredTagAndName()} ")
                .AddBlock("VS")
                .AddBlock($"{_Showdown.Match.TeamB.GetColoredTagAndName()} ")
            )
            .AddSeparator()
            .AddLine("Currently linked:")
            .AddLine(line => line
                .AddBlock($"{_teamA.GetTag()} ", format => format.Color(_teamA.Color))
                .AddBlock($"{_teamA.GetLinkedRacersToString()}")
            )
            .AddLine(line => line
                .AddBlock($"{_teamB.GetTag()} ", format => format.Color(_teamB.Color))
                .AddBlock($"{_teamB.GetLinkedRacersToString()}")
            )
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("All Racers are linked to their Teams!",
                    format => format.Color("#00ff00")
                )
            )
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("Continue to")
                .AddBlock("'Select Initiative'", block => block.Color("#ffff00"))
                .AddBlock("in")
                .AddBlock($"{countdownTime}", block => block.Color("#00ff00"))
                .AddBlock("seconds...")
            )
            .AddSeparator();
        msg.Send();
    }

    private void OnLinkRacerToTeam(ulong steamId)
    {
        string steamName = ZeepkistNetworkService.GetSteamNameFromSteamId(steamId);
        Racer racer = new Racer(steamId, steamName);
        _currentTeam.AddRacer(racer); // Add racer to current team

        CheckIfRacersAreLinked(); // Check again after each link
    }

    private void SendLinkingStatusMessage()
    {
        new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Waiting for everyone in")
                .AddBlock($"{_currentTeam.GetTag()} ", format => format.Color($"{_currentTeam.Color}"))
                .AddBlock("to type")
                .AddBlock("'!link'", f => f.Color("#ffff00").Bold())
                .AddBlock("in the chat")
            )
            .AddLine("Currently linked:")
            .AddLine(line => line
                .AddBlock($"{_teamA.GetTag()} ", format => format.Color(_teamA.Color))
                .AddBlock($"{_teamA.GetLinkedRacersToString()}")
            )
            .AddLine(line => line
                .AddBlock($"{_teamB.GetTag()} ", format => format.Color(_teamB.Color))
                .AddBlock($"{_teamB.GetLinkedRacersToString()}")
            )
            .Send();
    }
}