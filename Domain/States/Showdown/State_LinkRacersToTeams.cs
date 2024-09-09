using System;
using Showdown4.Commands;
using Showdown4.Domain.States.Showdown;
using Showdown4.Tmp;
using Showdown4.Utils;
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

    public State_LinkRacersToTeams(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;


    public IStateMachine StateMachine { get; }

    public void Enter()
    {
        _setNextTeam = false;
        _teamA = _Showdown.CurrentMatch.TeamA;
        _teamB = _Showdown.CurrentMatch.TeamB;

        CommandLinkPlayerToTeam.CommandInvoked += OnLinkRacerToTeam;
    }

    public void Execute()
    {
        CheckIfRacersAreLinked();
        new ServerMessage()
            .ShowdownHeader()
            .SetLineEffects(sle => sle
                .FontSize(20)
                .Italic()
            )
            .AddLine(l => l
                .AddContent(c => c
                    .AddText("Match set to:")
                    .Italic()
                ))
            .SetLineEffects(sle => sle
                .FontSize(20)
                .Italic()
            )
            .AddLine(l => l
                .AddContent(c => c
                    .AddText($"{_Showdown.CurrentMatch.TeamA.GetNameWithTag()} ")
                    .Color($"{_Showdown.CurrentMatch.TeamA.Color}")
                )
                .AddContent(c => c
                    .AddText("VS ")
                ).AddContent(c => c
                    .AddText($"{_Showdown.CurrentMatch.TeamB.GetNameWithTag()} ")
                    .Color($"{_Showdown.CurrentMatch.TeamB.Color}")
                )
            )
            .AddSeparator() // Separator line with <br> in front if necessary
            .AddLine(r => r
                .AddContent(c => c
                    .AddText("Linking Steam Accounts to Teams..."))
            )
            .AddSeparator()
            .AddLine(r => r
                .AddContent(c => c
                    .AddText("Waiting for everyone in "))
                .AddContent(c => c
                    .AddText($"{_currentTeam.GetTag()} ")
                    .Color($"{_currentTeam.Color}"))
                .AddContent(c => c
                    .AddText("to type <color=#ff0000>'!link'</color> in the chat"))
            ).AddLine(r => r
                .AddContent(c => c
                    .AddText("Currently linked: ")
                )
            ).AddLine(r => r
                .AddContent(c => c
                    .AddText($"{_teamA.GetTag()} ")
                    .Color(_teamA.Color))
                .AddContent(c => c
                    .AddText($"{_teamA.GetLinkedRacersToString()}"))
            ).AddLine(r => r
                .AddContent(c => c
                    .AddText($"{_teamB.GetTag()} ")
                    .Color(_teamB.Color)
                )
                .AddContent(c => c
                    .AddText($"{_teamB.GetLinkedRacersToString()}"))
            )
            .Send(); // Send the message
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
        _Showdown.ShowdownTimer.Stop();
        _Showdown.ShowdownTimer.Start();
        _Showdown.ShowdownTimer.Tick += OnTimerTick;
    }

    private void OnTimerTick()
    {
        if (_Showdown.ShowdownTimer.Ticks % 4 == 0)
        {
            new ServerMessage()
                .ShowdownHeader()
                .SetLineEffects(sle => sle
                    .FontSize(20)
                    .Italic()
                )
                .AddLine(l => l
                    .AddContent(c => c
                        .AddText("Match set to:")
                        .Italic()
                    ))
                .SetLineEffects(sle => sle
                    .FontSize(20)
                    .Italic()
                )
                .AddLine(l => l
                    .AddContent(c => c
                        .AddText($"{_Showdown.CurrentMatch.TeamA.GetNameWithTag()} ")
                        .Color($"{_Showdown.CurrentMatch.TeamA.Color}")
                    )
                    .AddContent(c => c
                        .AddText("VS ")
                    ).AddContent(c => c
                        .AddText($"{_Showdown.CurrentMatch.TeamB.GetNameWithTag()} ")
                        .Color($"{_Showdown.CurrentMatch.TeamB.Color}")
                    )
                )
                .AddSeparator() // Separator line with <br> in front if necessary
                .AddLine(r => r
                    .AddContent(c => c
                        .AddText("Linking Steam Accounts to Teams..."))
                )
                .AddSeparator()
                .AddLine(r => r
                    .AddContent(c => c
                        .AddText("Waiting for everyone in "))
                    .AddContent(c => c
                        .AddText($"{_currentTeam.GetTag()} ")
                        .Color($"{_currentTeam.Color}"))
                    .AddContent(c => c
                        .AddText("to type <color=#ff0000>'!link'</color> in the chat"))
                ).AddLine(r => r
                    .AddContent(c => c
                        .AddText("Currently linked: ")
                    )
                ).SetLineEffects(line => line
                    .Color(_teamA.Color)).AddLine(r => r
                    .AddContent(c => c
                        .AddText($"{_teamA.GetTag()} ")
                        .Color(_teamA.Color))
                    .AddContent(c => c
                        .AddText($"{_teamA.GetLinkedRacersToString()}")
                    )
                )
                .SetLineEffects(line => line
                    .Color(_teamB.Color))
                .AddLine(r => r
                    .AddContent(c => c
                        .AddText($"{_teamB.GetTag()} ")
                    )
                    .AddContent(c => c
                        .AddText($"{_teamB.GetLinkedRacersToString()}")
                    )
                )
                .AddSeparator()
                .AddLine(r => r
                    .AddContent(c => c
                        .AddText($"All steam accounts are linked to their teams! Starting Draft Phase in: {5 - _Showdown.ShowdownTimer.Ticks / 4}")
                        .Color("#00ff00"))
                )
                .Send(); // Send the message
        }

        if (_Showdown.ShowdownTimer.Ticks < 5 * 4)
        {
            return;
        }

        _Showdown.ShowdownTimer.Tick -= OnTimerTick;
        _Showdown.ShowdownTimer.Stop();
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