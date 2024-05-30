using System.Collections.Generic;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Messaging;

namespace Showdown4.Statemachine;

public class State_LinkRacersToTeams : IState
{
    private bool _isTeamASet;
    private bool _isTeamBSet;
    private bool _setNextTeam;
    private List<Team> _teams;


    public MatchStateMachine Context { get; set; }

    public void Enter(IStateMachine context)
    {
        _setNextTeam = false;
        Context = (MatchStateMachine)context;
        CommandLinkPlayerToTeam.CommandInvoked += OnLinkPlayerToTeam;
        _teams = ModStorage.Storage.LoadFromJson<TeamJsonWrapper>("Teams").Teams;
        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Match set to '{Context.CurrentMatch.TeamA.GetTag()} vs {Context.CurrentMatch.TeamB.GetTag()}'").NewLine()
                .DashedLine().build().Message
        );
        CheckIfRacersAreLinked();
    }

    public void Exit()
    {
        MessengerApi.Log("Match Preparation finished!");
        CommandLinkPlayerToTeam.CommandInvoked -= OnLinkPlayerToTeam;
    }

    private void CheckIfRacersAreLinked()
    {
        if (!_isTeamASet || !_isTeamBSet)
        {
            if (_isTeamASet && !_setNextTeam)
            {
                _setNextTeam = true;
                ChatApi.SendMessage(MessageFormatter.ClearChat() +
                                    $"Team {Context.CurrentMatch.TeamA.GetTag()} is set!" +
                                    MessageFormatter.PrintLine() +
                                    $"{Context.CurrentMatch.TeamB.GetTag()}: It's your turn! Every player of your team needs to write #link in the chat." +
                                    MessageFormatter.PrintLine());
            }

            string result = "/servermessage red 0 " +
                            "Match Preparation" +
                            MessageFormatter.PrintBreak() +
                            $"{Context.CurrentMatch.TeamA.GetNameWithTag()} ({Context.CurrentMatch.TeamA.GetLinkedRacersToString()}) VS '{Context.CurrentMatch.TeamB.GetNameWithTag()}' ({Context.CurrentMatch.TeamB.GetLinkedRacersToString()}) ";
            ChatApi.SendMessage(result);
        }
        else
        {
            ChatApi.SendMessage(
                new ChatMessage.Builder().NewLine()
                    .DashedLine().NewLine()
                    .TextLine("Everyone is linked to their team. Waiting for Host for further instructions").NewLine()
                    .DashedLine()
                    .build().Message);
            Context.TransitionTo(Context, new State_PreRacing());
        }
    }

    private void OnLinkPlayerToTeam(ulong steamId)
    {
        string steamName = ZeepkistNetwork.PlayerList.FirstOrDefault(player => player.SteamID == steamId)
            ?.GetUserNameNoTag();

        string resultMessage;
        Team team = !_isTeamASet ? Context.CurrentMatch.TeamA : Context.CurrentMatch.TeamB;
        if (!_isTeamASet)
        {
            resultMessage = Context.CurrentMatch.TeamA.AddRacer(steamName, steamId);
            _isTeamASet = Context.CurrentMatch.TeamA.TeamCompleted;
        }
        else if (!_isTeamBSet)
        {
            resultMessage = Context.CurrentMatch.TeamB.AddRacer(steamName, steamId);
            _isTeamBSet = Context.CurrentMatch.TeamB.TeamCompleted;
        }
        else
        {
            ChatApi.SendMessage("Both teams are already set.");
            return;
        }

        resultMessage = "<br>" +
                        resultMessage +
                        "<br>" +
                        $"Currently linked: {team.GetLinkedRacersToString()}";
        ChatApi.SendMessage(resultMessage);
        CheckIfRacersAreLinked();
    }
}