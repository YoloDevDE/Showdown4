using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Service;
using ZeepSDK.Chat;

namespace Showdown4.Statemachine;

public class State_SetTeams : IState
{
    private List<Team> _teams;
    public MatchStateMachine Context { get; set; }

    public void Enter(IStateMachine context)
    {
        Context = (MatchStateMachine)context;

        CommandSetTeam.CommandInvoked += OnSetTeam;
        Context.CurrentMatch = new Match();

        _teams = ModStorage.Storage.LoadFromJson<TeamJsonWrapper>("Teams").Teams;


        ChatApi.AddLocalMessage("Register Team ->'#set teams <teamtag>, <teamtag>'");
        ChatApi.AddLocalMessage(TeamService.GetFormattedTeams(_teams));
        ChatApi.SendMessage("/settime 86400");
    }

    public void Exit()
    {
        CommandSetTeam.CommandInvoked -= OnSetTeam;
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

        Context.CurrentMatch.TeamA = teamA;
        Context.CurrentMatch.TeamB = teamB;

        ChatApi.AddLocalMessage($"Teams set: {teamA.Name} vs {teamB.Name}");
        Context.TransitionTo(Context, new State_LinkRacersToTeams());
    }
}