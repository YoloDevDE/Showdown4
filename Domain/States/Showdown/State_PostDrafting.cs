using System;
using Showdown4.Tmp;

namespace Showdown4.Domain.States.Showdown;

public class State_PostDrafting : IState
{
    private readonly CountdownTimer timer;

    public State_PostDrafting(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
        timer = new CountdownTimer(60);
    }

    private ShowdownStateMachine _Showdown => StateMachine as ShowdownStateMachine;
    private Team _teamA => _Showdown.Match.TeamA;
    private Team _teamB => _Showdown.Match.TeamB;


    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
    }

    public void Execute()
    {
    }

    public void Exit()
    {
    }
}