using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Domain.Entities;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepSDK.Chat;
using Match = Showdown4.Tmp.Match;
using Team = Showdown4.Tmp.Team;

namespace Showdown4.Domain.States;

public class State_SetTeams : State
{
    private MatchStateMachine _context;
    private List<Team> _teams;


    public override void Exit()
    {
        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Match set to '{_context.CurrentMatch.TeamA.GetTag()} vs {_context.CurrentMatch.TeamB.GetTag()}'").NewLine()
                .DashedLine().Build().Message
        );
        CommandSetTeam.CommandInvoked -= OnSetTeam;
    }

    public override void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;
        TeamService teamService = new TeamService();
        CommandSetTeam.CommandInvoked += OnSetTeam;

        _teams = ModStorage.Storage.LoadFromJson<TeamJsonWrapper>("Teams").Teams;


        ChatApi.AddLocalMessage("Register Team ->'#set teams <teamtag>, <teamtag>'");
        ChatApi.AddLocalMessage(teamService.GetFormattedTeams(_teams));
        ChatApi.SendMessage("/settime 86400");
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

        _context.CurrentMatch = new Match(teamA, teamB, 3);
        _context.TransitionTo(new State_LinkRacersToTeams());
    }
}