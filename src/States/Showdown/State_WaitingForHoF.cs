using System;
using Showdown4.Managers;
using ZeepSDK.Level;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class State_WaitingForHoF : IState
{
    public State_WaitingForHoF(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public IStateMachine StateMachine { get; }
    public event Action Finished;

    public void Enter()
    {
        RacingApi.RoundStarted += OnRoundStarted;
    }

    public void Execute()
    {
        if (LevelApi.CurrentLevel.UID.Equals(PlaylistManager.GetCurrentPlaylistLevel().UID))
        {
            OnRoundStarted();
        }
    }

    public void Exit()
    {
        RacingApi.RoundStarted -= OnRoundStarted;
    }

    private void OnRoundStarted()
    {
        Finished?.Invoke();
    }
}