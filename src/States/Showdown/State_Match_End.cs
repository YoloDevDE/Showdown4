using System;
using System.Collections;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

public class State_Match_End : IState
{
    private bool countdownStarted;
    private int countdownTime = 60; // Countdown duration set to 60 seconds

    public State_Match_End(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;
    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        if (!countdownStarted)
        {
            CoroutineManager.Instance.StartExternalCoroutine(Countdown());
        }
    }

    public void Execute()
    {
        Team winnerTeam = Showdown.Match.CurrentRound.GetWinnerTeam;

        // Decorated ServerMessage without emotes
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddSeparator() // Add a separator line
            .AddLine(line => line
                .AddBlock("**Match Over!** ", block => block.Bold().Color("#ff0000"))
            )
            .AddLine(line => line
                    .AddBlock($"{winnerTeam.GetNameWithTag()} ", block => block.Color(winnerTeam.Color).Bold())
                    .AddBlock("won the Match!", block => block.Color("#FFD700")) // Gold for celebration
            )
            .AddLine(line => line
                    .AddBlock("Final Score: ", block => block.Bold().Color("#ffffff"))
                    .AddBlock(Showdown.Match.Score(), block => block.Color("#00ff00")) // Green for the final score
            )
            .AddSeparator() // Another separator for visual clarity
            .AddLine(line => line
                    .AddBlock($"Starting new match in {countdownTime} seconds.", block => block.Bold().Color(countdownTime <= 10 ? "#ff0000" : "#00ff00")) // Red if time is <= 10 seconds
            )
            .AddSeparator(); // Final separator

        msg.Send();
    }

    public void Exit()
    {
        CoroutineManager.Instance.StopAllExternalCoroutines();
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private IEnumerator Countdown()
    {
        countdownStarted = true;

        while (countdownTime > 0)
        {
            Execute(); // Update the message with the current countdown time

            yield return new WaitForSeconds(1);
            countdownTime--;
        }

        Finished?.Invoke(); // End the state once the countdown is complete
    }
}