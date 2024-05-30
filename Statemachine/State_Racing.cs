using System.Linq;
using Showdown4.Entities;
using Showdown4.Service;
using Showdown4.Utils;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Statemachine;

public class State_Racing : IState
{
    private MatchStateMachine _context;
    private RoundEvaluator _roundEvaluator;
    private Team _teamA, _teamB;

    public void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;
        _teamA = _context.CurrentMatch.TeamA;
        _teamB = _context.CurrentMatch.TeamB;

        Round round = new Round(_teamA, _teamB, _context.CurrentMatch.RoundCounter);
        _context.CurrentMatch.Rounds.Add(round);

        _roundEvaluator = new RoundEvaluator(_teamA, _teamB, _context.CurrentMatch.Rounds.Last());
        _context.CurrentMatch.CurrentRound.RoundEvaluator = _roundEvaluator;
        ChatApi.SendMessage("/settime 300");
        ChatApi.SendMessage(new ChatMessage.Builder().ClearChat()
            .DashedLine().NewLine()
            .CenterTextLine($"Round {_context.CurrentMatch.RoundCounter} started").NewLine()
            .DashedLine().NewLine()
            .CenterTextLine($"{_teamA.GetTag()}").NewLine()
            .CenterTextLine($"{_teamA.RacerA.SteamName} | {_teamA.RacerB.SteamName}").NewLine()
            .CenterTextLine("vs").NewLine()
            .CenterTextLine($"{_teamB.RacerA.SteamName} | {_teamB.RacerB.SteamName}").NewLine()
            .CenterTextLine($"{_teamB.GetTag()}").NewLine()
            .DashedLine()
            .NewLine()
            .TextLine("Good Luck, Have Fun! :smile:").Build().Message
        );
        MyLobbyManager.SetServerMessage(
            ServerMessageColor.green,
            new ChatMessage.Builder()
                .TextLine($"Round {_context.CurrentMatch.RoundCounter}: started!").NewLine()
                .TextLine($"{_teamA.GetTag()} {_teamA.Wins}:{_teamB.Wins} {_teamB.GetTag()}").Build().Message);


        ZeepkistNetwork.PlayerResultsChanged += OnLeaderBoardUpdated;
        RacingApi.RoundEnded += OnRoundEnd;
    }

    public void Exit()
    {
        ZeepkistNetwork.PlayerResultsChanged -= OnLeaderBoardUpdated;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    private void OnRoundEnd()
    {
        _context.CurrentMatch.CurrentRoundWinner = _roundEvaluator.CurrentWinner;
        _context.CurrentMatch.CurrentRoundLoser = _roundEvaluator.CurrentLoser;
        _context.TransitionTo(_context, new State_PostRacing());
    }

    private void OnLeaderBoardUpdated(ZeepkistNetworkPlayer netRacer)
    {
        _roundEvaluator.UpdateResults(netRacer);
        if (ZeepkistNetwork.CurrentLobby.GameState == 0)
        {
            _roundEvaluator.EvaluateCurrentTeams();
            if (ZeepkistNetwork.CurrentLobby != null && ZeepkistNetwork.CurrentLobby.GameState == 0)
            {
                _context.CurrentMatch.UpdateRacingScoreboard(_roundEvaluator.CurrentWinner, _roundEvaluator.CurrentLoser);
            }
        }
    }
}