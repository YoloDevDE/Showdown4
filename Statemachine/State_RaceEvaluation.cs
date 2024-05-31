using Showdown4.Tmp;

namespace Showdown4.Statemachine;

internal class State_RaceEvaluation : IState
{
    private MatchStateMachine _context;
    private Round _round;

    public void Enter(IStateMachine context)
    {
        // _context = (MatchStateMachine)context;
        // _round = _context.CurrentMatch.GetCurrentRound();
        //
        // Team winner = _round.RoundEvaluator.CurrentWinner;
        // Team loser = _round.RoundEvaluator.CurrentLoser;
        // if (_context.CurrentMatch.EvaluateMatchWinner() != null)
        // {
        //     ChatApi.SendMessage($"/servermessage green 0 {_context.CurrentMatch.MatchWinner.GetNameWithTag()} has won! :party:");
        //     ChatApi.SendMessage($"Match over!<br>Result:<br>{_context.CurrentMatch.MatchWinner.GetTag()} {_context.CurrentMatch.MatchWinner.Wins}:{_context.CurrentMatch.MatchLoser.Wins} {_context.CurrentMatch.MatchLoser.GetTag()}" +
        //                         $"<br><br>Congratulations {_context.CurrentMatch.MatchWinner.GetNameWithTag()}! You won the Match!");
        //     _context.StopStateMachine();
        // }
        // else
        // {
        //     ChatApi.SendMessage(
        //         new ChatMessage.Builder().NewLine()
        //             .DashedLine().NewLine()
        //             .TextLine($"{_context.CurrentMatch.CurrentRoundWinner.GetNameWithTag()} won Round {_context.CurrentMatch.CurrentRound.RoundNumber} :party:").NewLine()
        //             .DashedLine().NewLine()
        //             .TextLine("Results:").NewLine()
        //             .TextLine(TableFormatter.FormattingScoreboard(
        //                 [
        //                     [winner.GetTag(), _round.RoundEvaluator.GetAverageTime(winner).GetFormattedTime(), ""],
        //                     [
        //                         loser.GetTag(),
        //                         _round.RoundEvaluator.GetAverageTime(loser).GetFormattedTime(), MessageFormatter.FormatTimestampDifference(_round.RoundEvaluator.GetAverageTime(winner) - _round.RoundEvaluator.GetAverageTime(loser))
        //                     ]
        //                 ]
        //             )).NewLine()
        //             .TextLine("Standings:").NewLine()
        //             .TextLine($"{winner.GetTag()} {winner.Wins}:{loser.Wins} {loser.GetTag()}")
        //             .Build().Message);
        //     _context.TransitionTo(_context, new State_PreRacing());
        // }
    }

    public void Exit()
    {
    }
}