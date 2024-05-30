using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Statemachine;

internal class State_PostRacing : IState
{
    private MatchStateMachine _context;

    public void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;
        RacingApi.LevelLoaded += OnLevelLoaded;

        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Round {_context.CurrentMatch.CurrentRound.RoundNumber} over!!").NewLine()
                .DashedLine().Build().Message);
    }

    public void Exit()
    {
        RacingApi.LevelLoaded -= OnLevelLoaded;
    }

    private void OnLevelLoaded()
    {
        _context.TransitionTo(_context, new State_RaceEvaluation());
    }
}