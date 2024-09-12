using System;
using Showdown4.Commands;
using Showdown4.Domain.States.Showdown;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Messaging;

namespace Showdown4.Domain.States;

public class State_LinkRacersToTeams : IState
{
    private readonly TeamService _teamService = new TeamService();
    private readonly ZeepkistNetworkService _zeepkistNetworkService = new ZeepkistNetworkService();
    private bool _isTeamASet;
    private bool _isTeamBSet;

    private MatchService _matchService = new MatchService();
    private RoundService _roundService = new RoundService();
    private bool _setNextTeam;
    private Team _teamA, _teamB, _currentTeam;

    private bool test = true;

    public State_LinkRacersToTeams(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;


    public IStateMachine StateMachine { get; }

    public void Enter()
    {
        _setNextTeam = false;
        _teamA = _Showdown.Match.TeamA;
        _teamB = _Showdown.Match.TeamB;

        CommandLinkPlayerToTeam.CommandInvoked += OnLinkRacerToTeam;
    }

    public void Execute()
    {
        CheckIfRacersAreLinked();
        new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Match set to:", format =>
                    format
                        .Italic()
                )
            )
            .AddLine(line => line
                .Bold()
                .AddBlock($"{_Showdown.Match.TeamA.GetNameWithTag()} ", format =>
                    format
                        .Color($"{_Showdown.Match.TeamA.Color}")
                )
                .AddBlock("VS ")
                .AddBlock($"{_Showdown.Match.TeamB.GetNameWithTag()} ", format =>
                    format
                        .Color($"{_Showdown.Match.TeamB.Color}")
                )
            )
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("Linking Steam Accounts to Teams...")
            )
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("Waiting for everyone in ")
                .AddBlock($"{_currentTeam.GetTag()} ", format =>
                    format
                        .Color($"{_currentTeam.Color}"))
                .AddBlock("to type ")
                .AddBlock("'!link' ", format => format.Color("#ff0000"))
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
            .Send(); // Send the message
        if (test)
        {
            test = false;
            OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID);
            OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID);
            OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID);
            OnLinkRacerToTeam(ZeepkistNetwork.LocalPlayer.SteamID);
        }
    }

    public void Exit()
    {
        MessengerApi.Log("Match Preparation finished!");
        CommandLinkPlayerToTeam.CommandInvoked -= OnLinkRacerToTeam;
    }

    public event Action Finished;


    public void CheckIfRacersAreLinked()
    {
        if (_teamA.Racers.Count < _teamA.MaxTeamSize)
        {
            _currentTeam = _teamA;
        }
        else if (_teamB.Racers.Count < _teamB.MaxTeamSize)
        {
            _currentTeam = _teamB;
        }
        else
        {
            FadeOut();
        }
    }

    private void FadeOut()
    {
        _Showdown.Timer.Start();
        _Showdown.Timer.Tick += OnTimerTick;
    }

    private void OnTimerTick()
    {
        if (_Showdown.Timer.Ticks % 4 == 0)
        {
            new ServerMessage()
                .ShowdownHeader()
                .AddLine(line => line
                    .AddBlock("Match set to:", format =>
                        format
                            .Italic()
                    )
                )
                .AddLine(line => line
                    .Bold()
                    .AddBlock($"{_Showdown.Match.TeamA.GetNameWithTag()} ", format =>
                        format
                            .Color($"{_Showdown.Match.TeamA.Color}")
                    )
                    .AddBlock("VS ")
                    .AddBlock($"{_Showdown.Match.TeamB.GetNameWithTag()} ", format =>
                        format
                            .Color($"{_Showdown.Match.TeamB.Color}")
                    )
                )
                .AddSeparator()
                .AddLine(line => line
                    .AddBlock("Linking Steam Accounts to Teams...")
                )
                .AddSeparator()
                .AddLine(line => line
                    .AddBlock("Waiting for everyone in ")
                    .AddBlock($"{_currentTeam.GetTag()} ", format =>
                        format
                            .Color($"{_currentTeam.Color}"))
                    .AddBlock("to type ")
                    .AddBlock("'!link' ", format => format.Color("#ff0000"))
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
                .AddSeparator()
                .AddLine(line => line
                    .AddBlock($"All steam accounts are linked to their teams! Starting Draft Phase in: {5 - _Showdown.Timer.Ticks / 4}", format =>
                        format
                            .Color("#00ff00"))
                )
                .Send(); // Send the message
        }

        if (_Showdown.Timer.Ticks < 5 * 4)
        {
            return;
        }

        _Showdown.Timer.Tick -= OnTimerTick;
        _Showdown.Timer.Stop();
        Finished?.Invoke();
    }

    private void OnLinkRacerToTeam(ulong steamId)
    {
        string steamName = _zeepkistNetworkService.GetSteamNameFromSteamId(steamId);
        Racer racer = new Racer(steamId, steamName);
        // First fill teamA then fill teamB
        _teamService.AddRacer(_currentTeam, racer);
        Execute();
    }
}