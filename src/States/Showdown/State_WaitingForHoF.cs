using Showdown4.Config;
using Showdown4.Managers;
using ZeepSDK.Level;

namespace Showdown4.States.Showdown;

public class StateWaitingForHoF(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
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
	}

	public override void OnRoundStarted()
	{
		InvokeFinish();
	}
}