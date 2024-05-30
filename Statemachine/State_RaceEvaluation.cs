using Showdown4.Utils;
using ZeepSDK.Chat;

namespace Showdown4.Statemachine;

internal class State_RaceEvaluation : IState
{
    private MatchStateMachine _context;

    public void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;
        if (_context.CurrentMatch.EvaluateMatchWinner() != null)
        {
            ChatApi.SendMessage($"/servermessage green 0 {_context.CurrentMatch.MatchWinner.GetNameWithTag()} has won! :party:");
            ChatApi.SendMessage($"Match over!<br>Result:<br>{_context.CurrentMatch.MatchWinner.GetTag()} {_context.CurrentMatch.MatchWinner.Wins}:{_context.CurrentMatch.MatchLoser.Wins} {_context.CurrentMatch.MatchLoser.GetTag()}" +
                                $"<br><br>Congratulations {_context.CurrentMatch.MatchWinner.GetNameWithTag()}! You won the Match!");
            _context.StopStateMachine();
        }
        else
        {
            ChatApi.SendMessage(
                MessageFormatter.ClearChat() +
                MessageFormatter.PrintLine() +
                $"{_context.CurrentMatch.CurrentRoundWinner.GetNameWithTag()} won last Round!" +
                MessageFormatter.FormatRoundResult(_context.CurrentMatch.CurrentRound) +
                MessageFormatter.PrintLine() +
                $"Standings:<br>{_context.CurrentMatch.CurrentRoundWinner.GetTag()} {_context.CurrentMatch.CurrentRoundWinner.Wins}:{_context.CurrentMatch.CurrentRoundLoser.Wins} {_context.CurrentMatch.CurrentRoundLoser.GetTag()}");
            _context.TransitionTo(_context, new State_PreRacing());
        }
    }

    public void Exit()
    {
    }
}