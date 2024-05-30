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
    private MatchStateMachine _context;
    private bool _isTeamASet;
    private bool _isTeamBSet;
    private bool _setNextTeam;

    private Team _teamA, _teamB;

    public void Enter(IStateMachine context)
    {
        _setNextTeam = false;
        _context = (MatchStateMachine)context;

        _teamA = _context.CurrentMatch.TeamA;
        _teamB = _context.CurrentMatch.TeamB;

        CommandLinkPlayerToTeam.CommandInvoked += OnLinkPlayerToTeam;
        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Match set to '{_context.CurrentMatch.TeamA.GetTag()} vs {_context.CurrentMatch.TeamB.GetTag()}'").NewLine()
                .DashedLine().Build().Message
        );

        MyLobbyManager.SetServerMessage(ServerMessageColor.orange,
            new ChatMessage.Builder()
                .TextLine($"Round {_context.CurrentMatch.RoundCounter}: Intermission").NewLine()
                .TextLine($"{_teamA.GetTag()} {_teamA.Wins}:{_teamB.Wins} {_teamB.GetTag()}")
                .Build().Message);
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


                ChatApi.SendMessage(
                    new ChatMessage.Builder().NewLine()
                        .DashedLine().NewLine()
                        .TextLine($"Team {_context.CurrentMatch.TeamA.GetTag()} is set!").NewLine()
                        .TextLine($"{_context.CurrentMatch.TeamB.GetTag()}: It's your turn! Every player of your team needs to write #link in the chat.").NewLine()
                        .DashedLine().Build().Message
                );
            }

            MyLobbyManager.SetServerMessage(ServerMessageColor.orange,
                new ChatMessage.Builder()
                    .TextLine("Match Preparation").NewLine()
                    .TextLine($"{_teamA.GetNameWithTag()} ({_teamA.GetLinkedRacersToString()}) vs '{_teamB.GetNameWithTag()}' ({_teamB.GetLinkedRacersToString()})")
                    .Build().Message
            );
        }
        else
        {
            ChatApi.SendMessage(
                new ChatMessage.Builder().NewLine()
                    .DashedLine().NewLine()
                    .TextLine("Everyone is linked to their teams. Waiting for Host for further instructions").NewLine()
                    .DashedLine()
                    .Build().Message);
            _context.TransitionTo(_context, new State_PreRacing());
        }
    }

    private void OnLinkPlayerToTeam(ulong steamId)
    {
        string steamName = ZeepkistNetwork.PlayerList.FirstOrDefault(player => player.SteamID == steamId)
            ?.GetUserNameNoTag();

        string resultMessage;
        Team team = !_isTeamASet ? _context.CurrentMatch.TeamA : _context.CurrentMatch.TeamB;
        if (!_isTeamASet)
        {
            resultMessage = _context.CurrentMatch.TeamA.AddRacer(steamName, steamId);
            _isTeamASet = _context.CurrentMatch.TeamA.TeamCompleted;
        }
        else if (!_isTeamBSet)
        {
            resultMessage = _context.CurrentMatch.TeamB.AddRacer(steamName, steamId);
            _isTeamBSet = _context.CurrentMatch.TeamB.TeamCompleted;
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