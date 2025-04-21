using System;
using Showdown4.Entities;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

public class State_PreDraftCountdown : IState
{
    private const int CountdownDuration = 3;
    private Team _initiativeTeam;

    public State_PreDraftCountdown(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    private ShowdownStateMachine _showdown => StateMachine as ShowdownStateMachine;


    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        _initiativeTeam = _showdown.Match.Initiative;
        TimerUtility.StartCountdown(CountdownDuration, OnCountdownTick, OnCountdownComplete);
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        TimerUtility.StopCountdown();
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void OnCountdownTick(int remainingSeconds)
    {
        GenerateServerMessage(remainingSeconds).Send();
    }

    private void OnCountdownComplete()
    {
        InvokeFinish();
    }

    private ServerMessage GenerateServerMessage(int seconds)
    {
        return new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock($"Initiative has been given to {_initiativeTeam.GetColoredTagAndName()}").Bold())
            .AddLine(line => line
                .AddBlock($"{_initiativeTeam.GetTag()}", f => f.Color(_initiativeTeam.Color))
                .AddBlock("Prepare yourself! You will start to draft!"))
            .AddSeparator()
            .AddLine(line => line
                .AddBlock("Starting 'Draftphase' in")
                .AddBlock($"{seconds}", block => block.Color("#ff0000"))
                .AddBlock("seconds..."));
    }
}