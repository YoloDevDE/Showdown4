using System;
using System.Collections;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

public class State_LinkRacers : IState
{
    private const float CountdownDuration = 5f;
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
        _teamA = _Showdown.Match.TeamA;
        _teamB = _Showdown.Match.TeamB;
        CommandLinkPlayerToTeam.CommandInvoked += OnLinkRacerToTeam;
        CheckIfRacersAreLinked(); // Start linking process
    }

    public void Execute()
    {
        OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID); // Simulate linking for testing
        OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID); // Simulate linking for testing
        OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID); // Simulate linking for testing
        OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID); // Simulate linking for testing
    }

    public void Exit()
    {
        CommandLinkPlayerToTeam.CommandInvoked -= OnLinkRacerToTeam;
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
            yield return new WaitForSeconds(1f); // Wait for 1 second
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
                .AddBlock("All steam accounts are linked!",
                    format => format.Color("#00cc00")
                )
            )
            .AddLine(line => line
                .AddBlock("Starting Draft Phase in:")
                .AddBlock($"{TimeFormatter.FormatDuration(ticksLeft)}", format => format.Color("#cc0000"))
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