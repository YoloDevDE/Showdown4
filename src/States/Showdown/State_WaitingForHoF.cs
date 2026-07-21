using Showdown4.Managers;
using ZeepSDK.Level;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class StateWaitingForHoF : ShowdownStateBase
{
	public StateWaitingForHoF(IStateMachine stateMachine) : base(stateMachine)
	{
	}

	public override void Enter()
	{
		RacingApi.RoundStarted += OnRoundStarted;
	}

	public override void Execute()
	{
		if (LevelApi.CurrentLevel.UID.Equals(
			    PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)[0]
				    .UID))
		{
			OnRoundStarted();
		}
	}

	public override void Exit()
	{
		RacingApi.RoundStarted -= OnRoundStarted;
	}

	private void OnRoundStarted()
	{
		InvokeFinish();
	}
}