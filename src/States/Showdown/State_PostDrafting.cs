using System;
using System.Collections;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepSDK.Chat;
using Random = UnityEngine.Random;

// Required for using Coroutine

namespace Showdown4.States.Showdown;

public class State_PostDrafting : IState
{
    private bool _teamAReady;
    private bool _teamBReady;

    public State_PostDrafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _teamAReady = false;
        _teamBReady = false;

        AttachEvents();

        // Announce that the ready check has started
        ChatApi.SendMessage("Ready check started! Waiting for both teams to ready up.");

        // Start the simulated ready check coroutine
        CoroutineManager.Instance.StartExternalCoroutine(SimulateReadyCheck());
    }

    public void Execute()
    {
        // Regular updates like status checks can be done here, if needed
        SendReadyCheckStatus();
    }

    public void Exit()
    {
        DetachEvents();
    }

    // Event Handling
    private void AttachEvents()
    {
        // Optionally attach events like a real ready command
    }

    private void DetachEvents()
    {
        // Optionally detach events like a real ready command
    }

    // Simulated Ready Check Coroutine
    private IEnumerator SimulateReadyCheck()
    {
        while (!_teamAReady || !_teamBReady) // Continue until both teams are ready
        {
            // Wait for a random amount of time between 2 and 5 seconds
            float waitTime = Random.Range(2f, 5f);
            yield return new WaitForSeconds(waitTime);

            // Simulate team readiness check
            SimulateTeamReadyCheck();

            // Send updated ready status after each check
            SendReadyCheckStatus();
        }

        // Once both teams are ready, finish the ready check
        FinishReadyCheck();
    }

    // Simulate random team readiness
    private void SimulateTeamReadyCheck()
    {
        // Randomly set the team readiness for demonstration purposes
        if (!_teamAReady && Random.value > 0.5f)
        {
            _teamAReady = true;
            ChatApi.SendMessage($"{_Showdown.Match.TeamA.GetNameWithTag()} is now ready!");
        }

        if (!_teamBReady && Random.value > 0.5f)
        {
            _teamBReady = true;
            ChatApi.SendMessage($"{_Showdown.Match.TeamB.GetNameWithTag()} is now ready!");
        }
    }

    private void FinishReadyCheck()
    {
        if (!_teamAReady)
        {
            ChatApi.SendMessage($"{_Showdown.Match.TeamA.GetNameWithTag()} failed to ready up in time!");
        }

        if (!_teamBReady)
        {
            ChatApi.SendMessage($"{_Showdown.Match.TeamB.GetNameWithTag()} failed to ready up in time!");
        }

        // After the ready check finishes, invoke the state transition
        Finished?.Invoke();
    }

    private void SendReadyCheckStatus()
    {
        // Create a message that shows the ready status of both teams
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Ready Check in progress.")
            )
            .AddLine(line => line
                .AddBlock($"{_Showdown.Match.TeamA.GetNameWithTag()}: ")
                .AddBlock(_teamAReady ? "Ready" : "Not Ready", f => f.Color(_teamAReady ? "#00ff00" : "#ff0000"))
            )
            .AddLine(line => line
                .AddBlock($"{_Showdown.Match.TeamB.GetNameWithTag()}: ")
                .AddBlock(_teamBReady ? "Ready" : "Not Ready", f => f.Color(_teamBReady ? "#00ff00" : "#ff0000"))
            );
        msg.Send();
    }
}