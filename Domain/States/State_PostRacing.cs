using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Domain.States;

internal class State_PostRacing : IState
{
    public State_PostRacing(IStateMachine stateMachine)
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
        RacingApi.LevelLoaded -= OnLevelLoaded;
    }

    public void Enter( )
    {
        RacingApi.LevelLoaded += OnLevelLoaded;

        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Round {_showdownStateMachine.CurrentMatch.RoundCounter()} over!!").NewLine()
                .DashedLine().Build().Message);
    }

    private void OnLevelLoaded()
    {
        StateMachine.TransitionTo(new State_RaceEvaluation(StateMachine));
    }
}

