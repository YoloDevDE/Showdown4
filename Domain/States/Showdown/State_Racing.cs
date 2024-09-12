using System;
using Showdown4.Domain.States.Showdown;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Domain.States;

public class State_Racing : IState
{
    private Team _teamA, _teamB;

    public State_Racing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _showdownStateMachine => StateMachine as ShowdownStateMachine;


    public IStateMachine StateMachine { get; }


    public void Execute()
    {
    }

    public void Exit()
    {
        ZeepkistNetwork.PlayerResultsChanged -= OnLeaderBoardUpdated;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    public event Action Finished;

    public void Enter()
    {
        _teamA = _showdownStateMachine.Match.TeamA;
        _teamB = _showdownStateMachine.Match.TeamB;

        Round round = new Round(_showdownStateMachine.Match.RoundCounter() + 1);
        _showdownStateMachine.Match.Rounds.Add(round);

        ChatApi.SendMessage("/settime 300");
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Round {_showdownStateMachine.Match.RoundCounter()} started").NewLine()
                .DashedLine().NewLine()
                .CenterTextLine($"{_teamA.GetTag()}").NewLine()
                .CenterTextLine("vs").NewLine()
                .CenterTextLine($"{_teamB.GetTag()}").NewLine()
                .DashedLine()
                .NewLine()
                .TextLine("Good Luck, Have Fun! :smile:").Build().Message
        );
        LobbyController.SetServerMessage(
            ServerMessageColor.green,
            new ChatMessage.Builder()
                .TextLine($"Round {_showdownStateMachine.Match.RoundCounter()}: started!").NewLine()
                .TextLine($"{_teamA.GetTag()} {_teamA.Wins}:{_teamB.Wins} {_teamB.GetTag()}").Build().Message);


        ZeepkistNetwork.PlayerResultsChanged += OnLeaderBoardUpdated;
        ZeepkistNetwork.LobbyGameStateChanged += LobbyGameStateChanged;

        RacingApi.RoundEnded += OnRoundEnd;
    }

    private void LobbyGameStateChanged()
    {
        ChatApi.SendMessage($"LobbyGameStateChanged  {PlayerManager.Instance.currentMaster.gotResultsOnlineThisRound}");
    }

    private void OnRoundEnd()
    {
    }

    private void OnLeaderBoardUpdated(ZeepkistNetworkPlayer netRacer)
    {
        ChatApi.SendMessage($"Update {ZeepkistNetwork.CurrentLobby.GameState}");
        if (ZeepkistNetwork.CurrentLobby.GameState != 0)
        {
            ChatApi.SendMessage("/servermessage red 0 test");
        }

        ChatApi.SendMessage($"Servermessage {PlayerManager.Instance.currentMaster.gotResultsOnlineThisRound}");
    }
}