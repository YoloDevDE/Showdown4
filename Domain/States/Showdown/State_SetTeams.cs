using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Domain.Entities;
using Showdown4.Domain.States.Showdown;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepSDK.Chat;
using Match = Showdown4.Tmp.Match;
using Team = Showdown4.Tmp.Team;

namespace Showdown4.Domain.States;

public class State_SetTeams : IState
{
    private List<Team> _teams;

    public State_SetTeams(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine ShowdownStateMachine => (ShowdownStateMachine)StateMachine;


    public IStateMachine StateMachine { get; }


    public event Action Finished;

    public void Enter()
    {
        TeamService teamService = new TeamService();
        CommandSetTeam.CommandInvoked += OnSetTeam;

        _teams = ModStorage.Storage.LoadFromJson<TeamJsonWrapper>("Teams").Teams;


        ChatApi.AddLocalMessage("Register Team ->'#set teams <teamtag>, <teamtag>'");
        ChatApi.AddLocalMessage(teamService.GetFormattedTeams(_teams));
        ChatApi.SendMessage("/settime 86400");
        ShowdownStateMachine.ShowdownTimer.Tick += OnTimerTick;
    }

    public void Execute()
    {
        OnTimerTick();
    }

    public void Exit()
    {
        ShowdownStateMachine.ShowdownTimer.Tick += OnTimerTick;
        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Match set to '{ShowdownStateMachine.CurrentMatch.TeamA.GetTag()} vs {ShowdownStateMachine.CurrentMatch.TeamB.GetTag()}'").NewLine()
                .DashedLine().Build().Message
        );
        CommandSetTeam.CommandInvoked -= OnSetTeam;
    }

    private void OnTimerTick()
    {
        new ServerMessage()
            .AddHeadline(h => h
                .AddContent(c => c
                    .AddText("Showdown ")
                    .Color("#ff0000")
                    .Bold()
                    .Underline()
                    .AllCaps()
                )
                .AddContent(c => c
                    .AddText("Season ")
                    .Color("#ffffff")
                    .Bold()
                    .Underline()
                    .AllCaps()
                )
                .AddContent(c => c
                    .AddText("4")
                    .Color("#ff8800")
                    .Bold()
                    .Underline()
                    .AllCaps()
                )
            )
            .AddLine(l => l
                .AddContent(c => c
                    .AddText("Waiting for Host to initiate the Match-Procedure")
                    .Color("#aaaaaa")
                    .Italic()
                )
            )
            .AddSeparator() // Separator line with <br> in front if necessary
            .Send(); // Send the message
    }

    private void OnSetTeam(string arg)
    {
        string[] teamTags = arg.Trim().Split(",");
        if (teamTags.Length != 2)
        {
            ChatApi.AddLocalMessage("Please provide exactly two team tags.");
            return;
        }

        Team teamA = _teams.FirstOrDefault(team =>
            team.Tag.Equals(teamTags[0].Trim(), StringComparison.OrdinalIgnoreCase));
        Team teamB = _teams.FirstOrDefault(team =>
            team.Tag.Equals(teamTags[1].Trim(), StringComparison.OrdinalIgnoreCase));

        if (teamA == null || teamB == null)
        {
            ChatApi.AddLocalMessage("One or both team tags are invalid.");
            return;
        }

        ShowdownStateMachine.CurrentMatch = new Match(teamA, teamB, 3);
        Finished?.Invoke();
    }
}