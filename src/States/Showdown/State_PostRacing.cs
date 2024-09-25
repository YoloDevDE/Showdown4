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

    public void Enter()
    {
        RacingApi.LevelLoaded += OnLevelLoaded;
    }

    public void Execute()
    {
        Team winnerTeam = _showdownStateMachine.Match.CurrentRound.GetWinnerTeam;
        winnerTeam.AddWin();

        ChatApi.SendMessage(
            new ChatMessage.Builder().NewLine()
                .DashedLine().NewLine()
                .TextLine($"Round {_showdownStateMachine.Match.RoundCounter()} over!!").NewLine()
                .TextLine($"Team {winnerTeam.GetNameWithTag()} scored!").NewLine()
                .TextLine("Current Standing:").NewLine()
                .TextLine($"{_showdownStateMachine.Match.Score()}").NewLine()
                .DashedLine().NewLine()
                .TextLine($"Moving to '{_showdownStateMachine.Match.CurrentDraft.PickedLevels[_showdownStateMachine.Match.RoundCounter()].Level.Name}'").NewLine()
                .DashedLine().Build().Message);
    }

    public void Exit()
    {
        RacingApi.LevelLoaded -= OnLevelLoaded;
    }

    public event Action Finished;


    private void OnLevelLoaded()
    {
        Finished?.Invoke();
    }
}