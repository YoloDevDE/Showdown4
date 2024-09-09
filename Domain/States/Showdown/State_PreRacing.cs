using System;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.Domain.States.Showdown;

public class State_PreRacing : IState
{
    private Team _teamA, _teamB;

    public State_PreRacing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => (ShowdownStateMachine)StateMachine;


    public void Execute()
    {
    }

    public void Exit()
    {
        RacingApi.LevelLoaded -= OnLevelLoaded;
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _teamA = _Showdown.CurrentMatch.TeamA;
        _teamB = _Showdown.CurrentMatch.TeamB;

        ChatApi.SendMessage("/settime 86400");


        RacingApi.LevelLoaded += OnLevelLoaded;
        RacingApi.RoundEnded += OnRoundEnd;
    }

    private void OnRoundEnd()
    {
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Starting Round {_Showdown.CurrentMatch.RoundCounter()}").NewLine()
                .DashedLine().Build().Message
        );
    }

    private void OnLevelLoaded()
    {
    }
}