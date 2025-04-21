using System;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

public class State_DraftComplete : IState
{
    private const int Countdown = 3;
    private int _draftCompleteCountDownTick = Countdown;
    private ShowdownStateMachine _showdown;

    public State_DraftComplete(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        _showdown = StateMachine as ShowdownStateMachine;
        StartDraftCompleteCountdown();
    }

    public void Execute()
    {
        SendDraftCompleteMessage();
    }

    public void Exit()
    {
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void StartDraftCompleteCountdown()
    {
        TimerUtility.StartCountdown(Countdown, OnDraftCompleteTick, InvokeFinish);
    }

    private void OnDraftCompleteTick(int remainingSeconds)
    {
        _draftCompleteCountDownTick = remainingSeconds;
        Execute();
    }

    private void SendDraftCompleteMessage()
    {
        ServerMessage tmp = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line =>
            {
                line.AddBlock("Draft complete!")
                    .AddBlock("Continue to")
                    .AddBlock("'Pre-Racing'", block => block.Color("#ffff00"))
                    .AddBlock("in")
                    .AddBlock($"{_draftCompleteCountDownTick}", block => block.Color("#00ff00"))
                    .AddBlock("seconds...");
            });
        tmp.Send();
    }
}