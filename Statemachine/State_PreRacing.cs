using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Statemachine;

public class State_PreRacing : IState
{
    private MatchStateMachine _context;

    public void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;
        ChatApi.SendMessage("/settime 86400");
        ChatApi.SendMessage(
            $"/servermessage yellow 0 Round {_context.CurrentMatch.RoundCounter}: Intermission" +
            $"<br>{_context.CurrentMatch.TeamA.GetNameWithTag()} {_context.CurrentMatch.TeamA.Wins}:{_context.CurrentMatch.TeamB.Wins} {_context.CurrentMatch.TeamB.GetNameWithTag()}");

        RacingApi.LevelLoaded += OnLevelLoaded;
        RacingApi.RoundEnded += OnRoundEnd;
    }


    public void Exit()
    {
        RacingApi.LevelLoaded -= OnLevelLoaded;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    private void OnRoundEnd()
    {
        ChatApi.SendMessage("Starting Round " + _context.CurrentMatch.RoundCounter);
    }

    private void OnLevelLoaded()
    {
        ChatApi.SendMessage(MessageFormatter.ClearChat() +
                            "<br>" +
                            $"Round {_context.CurrentMatch.RoundCounter} started" +
                            "<br>" +
                            $"{_context.CurrentMatch.TeamA.GetNameWithTag()}" +
                            "<br>" +
                            "vs" +
                            "<br>" +
                            $"{_context.CurrentMatch.TeamB.GetNameWithTag()}" +
                            "<br>" +
                            "<br>" +
                            "Good Luck, Have Fun! :smile:");
        _context.TransitionTo(_context, new State_Racing());
    }
}