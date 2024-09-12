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

namespace Showdown4.Domain.States.Showdown;

public class State_SetupMatch : IState
{
    private List<Team> _teams;

    public State_SetupMatch(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine ShowdownStateMachine => (ShowdownStateMachine)StateMachine;


    public IStateMachine StateMachine { get; }


    public event Action Finished;

    public void Enter()
    {
        TeamService teamService = new TeamService();
        CommandSetupMatch.CommandInvoked += OnSetTeam;

        _teams = ModStorage.Storage.LoadFromJson<TeamJsonWrapper>("Teams").Teams;


        // ChatApi.AddLocalMessage("Setup Match ->'/sd match <teamtag>,<teamtag>'");
        // ChatApi.AddLocalMessage(teamService.GetFormattedTeams(_teams));
        ChatApi.SendMessage("/settime 86400");
        ShowdownStateMachine.Timer.Tick += OnTimerTick;
        ShowdownStateMachine.Timer.Start();
    }

    public void Execute()
    {
        OnTimerTick();
    }

    public void Exit()
    {
        ShowdownStateMachine.Timer.Stop();
        ShowdownStateMachine.Timer.Tick -= OnTimerTick;
        CommandSetupMatch.CommandInvoked -= OnSetTeam;
    }

    private void OnTimerTick()
    {
        new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Waiting for Host to setup the Match")
            )
            .AddSeparator() // Separator line with <br> in front if necessary
            .Send(); // Send the message

        if ((ShowdownStateMachine.Timer.Ticks + 1) % 4 * 5 == 0)
        {
            OnSetTeam("NIL,RUSH");
        }
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

        ShowdownStateMachine.Match = new Match(teamA, teamB);
        Finished?.Invoke();
    }
}