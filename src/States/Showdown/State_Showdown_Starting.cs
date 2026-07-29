using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;

namespace Showdown4.States.Showdown;

/// <summary>
///     The very first state of a showdown: it switches the server to the intermission playlist and
///     makes sure the lobby is on the Hall of Fame level before anything else happens.
/// </summary>
public class StateShowdownStarting(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		PlaylistManager.SetServerPlaylist(MyConfig.IntermissionLevelPlaylistNameConfig.Value);

		bool isOnIntermissionLevel = PlaylistManager.IsOnIntermissionLevel();
		if (!isOnIntermissionLevel)
		{
			ChatCommandService.SkipToLevel(0);
		}

		ChatMessage.SendCustomMessage(new ChatMessage.Builder()
			.ClearChat()
			.NewLine()
			.DashedLine()
			.NewLine()
			.TextLine($"Showdown Season {MyConfig.SeasonNumberConfig.Value} started")
			.NewLine()
			.DashedLine()
			.NewLine()
			.TextLine(isOnIntermissionLevel ? "Already on HoF :smile:" : "Skipping to HoF...")
			.Build().Message);

		InvokeFinish();
	}

	public override IState GetNextState()
	{
		return PlaylistManager.IsOnIntermissionLevel()
			? new StateSetupMatch(StateMachine)
			: new StateWaitingForHoF(StateMachine);
	}
}