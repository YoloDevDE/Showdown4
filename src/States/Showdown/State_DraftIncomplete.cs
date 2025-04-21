using System;
using Showdown4.Utils;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class State_DraftIncomplete : IState
{
    private ShowdownStateMachine _showdown;

    public State_DraftIncomplete(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        _showdown = StateMachine as ShowdownStateMachine;
        WaitForRandomMapSelection();
    }

    public void Execute()
    {
        SendDraftIncompleteMessage();
    }

    public void Exit()
    {
    }

    public void InvokeFinish()
    {
        Finished?.Invoke();
    }

    private void WaitForRandomMapSelection()
    {
        // Wait for a random map to be selected or additional actions
        ChatApi.SendMessage("Draft incomplete! Waiting for Host to initiate Random Map Selection");
    }

    private void SendDraftIncompleteMessage()
    {
        ServerMessage tmp = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Draft incomplete! Waiting for further actions."));
        tmp.Send();
    }
}