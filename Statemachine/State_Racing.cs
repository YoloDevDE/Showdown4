using System.Linq;
using Showdown4.Service;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Statemachine;

public class State_Racing : IState
{
    private MatchStateMachine _context;
    private RoundEvaluator _roundEvaluator;

    public void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;

        Round round = new Round(_context.CurrentMatch.TeamA, _context.CurrentMatch.TeamB, _context.CurrentMatch.RoundCounter);
        _context.CurrentMatch.Rounds.Add(round);
        _roundEvaluator = new RoundEvaluator(_context.CurrentMatch.TeamA, _context.CurrentMatch.TeamB, _context.CurrentMatch.Rounds.Last());
        _context.CurrentMatch.CurrentRound.RoundEvaluator = _roundEvaluator;


        ChatApi.SendMessage(
            $"/servermessage yellow 0 Round {_context.CurrentMatch.RoundCounter}: LIVE" +
            $"<br>{_context.CurrentMatch.TeamA.GetTag()} {_context.CurrentMatch.TeamA.Wins}:{_context.CurrentMatch.TeamB.Wins} {_context.CurrentMatch.TeamB.GetTag()}");

        ChatApi.SendMessage("/settime 300");
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