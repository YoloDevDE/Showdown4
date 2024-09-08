using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Domain.States;

public class State_PreRacing : IState
{
    private Team _teamA, _teamB;

    public State_PreRacing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine ShowdownStateMachine => (ShowdownStateMachine)StateMachine;



    public void Execute()
    {
    }

    public void Exit()
    {
        RacingApi.LevelLoaded -= OnLevelLoaded;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    public IStateMachine StateMachine { get; }

    public void Enter()
    {
        _teamA = ShowdownStateMachine.CurrentMatch.TeamA;
        _teamB = ShowdownStateMachine.CurrentMatch.TeamB;

        ChatApi.SendMessage("/settime 86400");
        LobbyController.SetServerMessage(ServerMessageColor.orange,
            new ChatMessage.Builder()
                .TextLine($"Round {ShowdownStateMachine.CurrentMatch.RoundCounter()}: Intermission").NewLine()
                .TextLine($"{_teamA.GetTag()} {_teamA.Wins}:{_teamB.Wins} {_teamB.GetTag()}")
                .Build().Message);

        RacingApi.LevelLoaded += OnLevelLoaded;
        RacingApi.RoundEnded += OnRoundEnd;
    }

    private void OnRoundEnd()
    {
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Starting Round {ShowdownStateMachine.CurrentMatch.RoundCounter()}").NewLine()
                .DashedLine().Build().Message
        );
    }

    private void OnLevelLoaded()
    {
        StateMachine.TransitionTo(new State_Racing(StateMachine));
    }
}