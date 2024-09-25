using System;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class State_Match_End : IState
{
    public State_Match_End(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;
    public IStateMachine StateMachine { get; }

    public event Action Finished;

    public void Enter()
    {
    }

    public void Execute()
    {
        ChatApi.SendMessage($"Match over! {Showdown.Match.Score()}");
    }

    public void Exit()
    {
    }
}