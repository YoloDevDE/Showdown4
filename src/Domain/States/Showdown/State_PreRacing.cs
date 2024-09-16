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
        RacingApi.RoundEnded -= OnRoundEnd;
    }

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _teamA = _Showdown.Match.TeamA;
        _teamB = _Showdown.Match.TeamB;

        ChatApi.SendMessage("/settime 86400");
        RacingApi.RoundEnded += OnRoundEnd;

        // Start the countdown and update the chat with the remaining seconds
        CoroutineStarter.Instance.StartCoroutine(CountDown.Seconds(10, UpdateCountdownMessage, SkipToNextLevel));
    }

    private void UpdateCountdownMessage(float secondsRemaining)
    {
        // Create a server message with formatted countdown message
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader() // Assuming ShowdownHeader is a custom header format you have
            .AddLine(line => line
                .AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
                .AddBlock("VS")
                .AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
            )
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("Race starts in")
                .AddBlock($"{secondsRemaining}", format => format.Bold()
                    .Color(secondsRemaining <= 10 ? "#ff0000" : "#ffffff")
                )
                .AddBlock("seconds")
            )
            .AddSeparator();

        msg.Send();
    }

    private void SkipToNextLevel()
    {
        ChatApi.SendMessage("/fs");
    }

    private void OnRoundEnd()
    {
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Starting Round {_Showdown.Match.RoundCounter()}").NewLine()
                .DashedLine().Build().Message
        );
        Finished?.Invoke();
    }
}