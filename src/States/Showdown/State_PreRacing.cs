using System;
using System.Collections;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistNetworking;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

// Required for using Coroutine

namespace Showdown4.States.Showdown;

public class State_PreRacing : IState
{
    private Team _teamA, _teamB;

    public State_PreRacing(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => (ShowdownStateMachine)StateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _teamA = _Showdown.Match.TeamA;
        _teamB = _Showdown.Match.TeamB;

        RacingApi.RoundEnded += OnRoundEnd;
        RacingApi.LevelLoaded += OnLevelLoaded;
        // Start the 10-second countdown and display it
        CoroutineManager.Instance.StartExternalCoroutine(StartCountdown(10));
    }

    public void Execute()
    {
        // Set the lobby time to 300 seconds (5 minutes)
        ChatApi.SendMessage("/settime 300");
    }

    public void Exit()
    {
        RacingApi.RoundEnded -= OnRoundEnd;

        RacingApi.LevelLoaded -= OnLevelLoaded;
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void OnLevelLoaded()
    {
        Finished?.Invoke();
    }

    private void UpdateCountdownMessage(int secondsRemaining)
    {
        // Create a server message with the match information and countdown in red
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
                .AddBlock(" VS ")
                .AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
            )
            .AddSeparator()
            .AddLine(line => line
                    .AddBlock("Race starts in: ")
                    .AddBlock($"{secondsRemaining} seconds", f => f.Bold().Color("#ff0000")) // Red countdown
            )
            .AddSeparator()
            .AddMessage(ShowPickedMaps());

        msg.Send();
    }

    private IEnumerator StartCountdown(int countdownDuration)
    {
        while (countdownDuration >= 0)
        {
            // Update the countdown message every second
            UpdateCountdownMessage(countdownDuration);

            // Wait for 1 second
            yield return new WaitForSeconds(1f);

            countdownDuration--;
        }

        // After countdown finishes, move to the next level
        SkipToNextLevel();
    }

    private void SkipToNextLevel()
    {
        ChatApi.SendMessage("/fs 0"); // Move to the next level
    }

    private ServerMessage ShowPickedMaps()
    {
        ServerMessage msg = new ServerMessage();
        msg
            .AddLine(line => line
                .AddBlock("Picked Maps:"));
        for (int index = 0; index < _Showdown.Match.CurrentDraft.PickedLevels.Count; index++)
        {
            int index1 = index;
            msg.AddLine(line =>
            {
                OnlineZeeplevel level = _Showdown.Match.CurrentDraft.PickedLevels[index1].Level;
                line
                    .AddBlock($"Round {index1 + 1}:")
                    .AddBlock($"'{level.Name}'", block => block.Color("#00ffff")
                    )
                    .Bold();
            });
        }

        return msg;
    }

    private void OnRoundEnd()
    {
        ChatApi.SendMessage(
            new ChatMessage.Builder().ClearChat()
                .DashedLine().NewLine()
                .CenterTextLine($"Starting Round {_Showdown.Match.RoundCounter() + 1}").NewLine()
                .DashedLine().Build().Message
        );
    }
}