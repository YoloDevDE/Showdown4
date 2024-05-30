using Showdown4.Entities;
using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Statemachine;

public class State_PreRacing : IState
{
    private MatchStateMachine _context;
    private Team _teamA, _teamB;


    public void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;
        _teamA = _context.CurrentMatch.TeamA;
        _teamB = _context.CurrentMatch.TeamB;

        ChatApi.SendMessage("/settime 86400");
        MyLobbyManager.SetServerMessage(ServerMessageColor.orange,
            new ChatMessage.Builder()
                .TextLine($"Round {_context.CurrentMatch.RoundCounter}: Intermission").NewLine()
                .TextLine($"{_teamA.GetTag()} {_teamA.Wins}:{_teamB.Wins} {_teamB.GetTag()}")
                .Build().Message);

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
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Starting Round {_context.CurrentMatch.RoundCounter}").NewLine()
                .DashedLine().Build().Message
        );
    }

    private void OnLevelLoaded()
    {
        _context.TransitionTo(_context, new State_Racing());
    }
}