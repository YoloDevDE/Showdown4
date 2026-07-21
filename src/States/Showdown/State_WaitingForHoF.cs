using System;
using Showdown4.Managers;
using ZeepSDK.Level;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class StateWaitingForHoF : IState
{
	public StateWaitingForHoF(IStateMachine stateMachine)
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
		if (LevelApi.CurrentLevel.UID.Equals(
			    PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)[0]
				    .UID))
			OnRoundStarted();
	}

	public void Exit()
	{
		RacingApi.RoundStarted -= OnRoundStarted;
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}

	private void OnRoundStarted()
	{
		Finished?.Invoke();
	}
}