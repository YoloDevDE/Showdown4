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
    private const int CountdownDuration = 10;
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

    private IEnumerator CountdownCoroutine(float countdown)
    {
        while (countdown > 0)
        {
            OnTimerTick((int)countdown); // Update message every second
            yield return new WaitForSeconds(1); // Wait for 1 second
            countdown--; // Decrease the countdown value
        }

        Finished?.Invoke(); // Notify when countdown finishes
    }

    private void OnTimerTick(int ticksLeft)
    {
        new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Match set to:", format => format.Italic())
            )
            .AddLine(line => line
                .Bold()
                .AddBlock($"{_Showdown.Match.TeamA.GetNameWithTag()} ",
                    format => format.Color($"{_Showdown.Match.TeamA.Color}"))
                .AddBlock("VS ")
                .AddBlock($"{_Showdown.Match.TeamB.GetNameWithTag()} ",
                    format => format.Color($"{_Showdown.Match.TeamB.Color}"))
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
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("All Racers are linked to their Teams!",
                    format => format.Color("#00ff00")
                )
            )
            .AddLine(line => line
                .AddBlock("Starting 'DraftPhase I' in:")
                .AddBlock($"{TimeFormatter.FormatDuration(ticksLeft)}", format => format.Color("#ff0000"))
            )
            .Send();
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
                .AddBlock("Waiting for everyone in ")
                .AddBlock($"{_currentTeam.GetTag()} ", format => format.Color($"{_currentTeam.Color}"))
                .AddBlock("to type '!link' in the chat")
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