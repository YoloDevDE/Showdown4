using System;
using Showdown4.Entities;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

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

    public event Action Finished;

    public void Enter()
    {
        RacingApi.LevelLoaded += OnLevelLoaded;

        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Round {_showdownStateMachine.Match.RoundCounter()} over!!").NewLine()
                .DashedLine().Build().Message);
    }

    private void OnLevelLoaded()
    {
    }
}