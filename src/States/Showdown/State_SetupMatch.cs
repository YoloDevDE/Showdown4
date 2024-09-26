using System;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Utils;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class State_SetupMatch : IState
{
    public State_SetupMatch(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine ShowdownStateMachine => (ShowdownStateMachine)StateMachine;
    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        CommandSetupMatch.CommandInvoked += OnSetTeam;
    }

    public void Execute()
    {
        ChatApi.SendMessage("/settime 86400");
        new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Waiting for Host to setup the Match"))
            .AddSeparator()
            .Send();
    }

    public void Exit()
    {
        CommandSetupMatch.CommandInvoked -= OnSetTeam;
    }


    private void OnSetTeam(string arg)
    {
        Team teamA = null;
        Team teamB = null;

        // Parse team tags and assign the teams directly here
        string[] teamTags = arg.Trim().Split(",");
        if (teamTags.Length != 2)
        {
            ChatApi.AddLocalMessage("Please provide exactly two team tags.");
            return;
        }
        //
        // teamA = _teams.FirstOrDefault(team => team.Tag.Equals(teamTags[0].Trim(), StringComparison.OrdinalIgnoreCase));
        // teamB = _teams.FirstOrDefault(team => team.Tag.Equals(teamTags[1].Trim(), StringComparison.OrdinalIgnoreCase));
        //
        // if (teamA == null || teamB == null)
        // {
        //     ChatApi.AddLocalMessage("One or both team tags are invalid.");
        //     return;
        // }

        teamA = new Team("cool mega nerds", "Nerd", "#ff00ff");
        teamB = new Team("red and fast", "Maki", "#ff0000");
        ShowdownStateMachine.Match = new Match(teamA, teamB);
        Finished?.Invoke();
    }
}