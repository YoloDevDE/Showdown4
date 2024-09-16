using System;
using Showdown4.Tmp;
using Showdown4.Utils;
using ZeepSDK.Chat;

namespace Showdown4.Domain.States.Showdown;

public class State_PostDrafting : IState
{
    private readonly CountdownTimer _readyCheckTimer;
    private bool _teamAReady;
    private bool _teamBReady;

    public State_PostDrafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        _readyCheckTimer = new CountdownTimer(10); // 10 second countdown
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;

    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
        _teamAReady = false;
        _teamBReady = false;

        AttachEvents();

        // Start the 10-second countdown
        _readyCheckTimer.Start();

        // Announce that the ready check has started
        ChatApi.SendMessage("Ready check started! Both teams have 10 seconds to ready up.");
    }

    public void Execute()
    {
        // Regular updates like countdown ticks can be sent here
        SendReadyCheckStatus();
    }

    public void Exit()
    {
        DetachEvents();
        _readyCheckTimer.Stop();
    }

    // Event Handling
    private void AttachEvents()
    {
        // CommandReady.CommandInvoked += OnTeamReady;
        _readyCheckTimer.CountdownTick += OnReadyCheckTimerTick;
        _readyCheckTimer.CountdownFinished += OnReadyCheckTimerFinished;
    }

    private void DetachEvents()
    {
        // CommandReady.CommandInvoked -= OnTeamReady;
        _readyCheckTimer.CountdownTick -= OnReadyCheckTimerTick;
        _readyCheckTimer.CountdownFinished -= OnReadyCheckTimerFinished;
    }

    private void OnTeamReady(ulong steamId)
    {
        // // Assuming each team has a method for checking if a player belongs to that team
        // if (_Showdown.Match.TeamA.IsPlayerOnTeam(steamId))
        // {
        //     _teamAReady = true;
        //     ChatApi.SendMessage($"{_Showdown.Match.TeamA.GetNameWithTag()} is ready!");
        // }
        // else if (_Showdown.Match.TeamB.IsPlayerOnTeam(steamId))
        // {
        //     _teamBReady = true;
        //     ChatApi.SendMessage($"{_Showdown.Match.TeamB.GetNameWithTag()} is ready!");
        // }

        // If both teams are ready, finish the state early
        if (_teamAReady && _teamBReady)
        {
            FinishReadyCheck();
        }
    }

    private void OnReadyCheckTimerTick()
    {
        SendReadyCheckStatus(); // Update status with the countdown timer
    }

    private void OnReadyCheckTimerFinished()
    {
        FinishReadyCheck();
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
        // Create a message that shows the ready status and the time left
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line
                .AddBlock("Ready Check in progress. Time remaining: ")
                .AddBlock($"{TimeFormatter.FormatDuration(_readyCheckTimer.SecondsLeft)}", f => f.Bold().Color("#00ff00"))
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