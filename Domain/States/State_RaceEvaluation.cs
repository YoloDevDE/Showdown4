using Showdown4.Tmp;

namespace Showdown4.Domain.States;

 public class State_RaceEvaluation : IState
{
    ShowdownStateMachine ShowdownStateMachine => StateMachine as ShowdownStateMachine;
    private Round _round;

    public State_RaceEvaluation(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }


    public void Execute()
    {
        
    }

    public void Exit()
    {
    }

    public IStateMachine StateMachine { get; }

    public void Enter()
    {

        // _round = ShowdownStateMachine.CurrentMatch.GetCurrentRound();
        //
        // Team winner = _round.RoundEvaluator.CurrentWinner;
        // Team loser = _round.RoundEvaluator.CurrentLoser;
        // if (ShowdownStateMachine.CurrentMatch.EvaluateMatchWinner() != null)
        // {
        //     ChatApi.SendMessage($"/servermessage green 0 {ShowdownStateMachine.CurrentMatch.MatchWinner.GetNameWithTag()} has won! :party:");
        //     ChatApi.SendMessage($"Match over!<br>Result:<br>{ShowdownStateMachine.CurrentMatch.MatchWinner.GetTag()} {ShowdownStateMachine.CurrentMatch.MatchWinner.Wins}:{ShowdownStateMachine.CurrentMatch.MatchLoser.Wins} {ShowdownStateMachine.CurrentMatch.MatchLoser.GetTag()}" +
        //                         $"<br><br>Congratulations {ShowdownStateMachine.CurrentMatch.MatchWinner.GetNameWithTag()}! You won the Match!");
        //     ShowdownStateMachine.StopStateMachine();
        // }
        // else
        // {
        //     ChatApi.SendMessage(
        //         new ChatMessage.Builder().NewLine()
        //             .DashedLine().NewLine()
        //             .TextLine($"{ShowdownStateMachine.CurrentMatch.CurrentRoundWinner.GetNameWithTag()} won Round {ShowdownStateMachine.CurrentMatch.CurrentRound.RoundNumber} :party:").NewLine()
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
        //     ShowdownStateMachine.TransitionTo(ShowdownStateMachine, new State_PreRacing());
        // }
    }
}

