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

    private int countdownTime = 10; // Countdown duration in seconds

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

        ServerMessage msg = new ServerMessage().ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Match over!")
                .AddBlock($"{winnerTeam.GetNameWithTag()} won the Match!", block => block.Color(winnerTeam.Color))
            )
            .AddLine(Showdown.Match.Score())
            .AddLine($"Starting new Match in {countdownTime} seconds.");

        msg.Send();
    }

    public void Exit()
    {
        CoroutineManager.Instance.StopAllExternalCoroutines();
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