using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

/// <summary>
///     Closes the match: the lobby timer is suspended, everyone may drive again and a countdown
///     shows how long it takes until the teams are kicked. This state is also the final state of the
///     showdown machine, so it can be entered without a match ever having been played.
/// </summary>
public class StateMatchEnd(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// From this many seconds left the kick countdown is coloured red.
	private const int LowTimeThresholdSeconds = 10;

	private int _countdownTime = MyConfig.MatchEndKickCountdownConfig.Value; // Countdown duration in seconds

	public override void Enter()
	{
		// Nothing is raced anymore, so the lobby timer must not run out.
		ChatCommandService.SuspendTimer();

		// The racing states leave everyone blocked from setting a time after the last sub-round.
		// The match is over, so give the lobby its freedom back.
		RacerResetService.UnblockEveryoneFromSettingTime();

		// Bring the lobby back to the Hall of Fame, exactly like StateShowdownStarting does. The
		// "+24h" race timer never runs out on its own, so without this the lobby would stay parked on
		// the last raced map and StateWaitingForHoF - which only waits - would never see its
		// RoundStarted event.
		PlaylistManager.SetServerPlaylist(MyConfig.IntermissionLevelPlaylistNameConfig.Value);
		if (!PlaylistManager.IsOnIntermissionLevel())
		{
			ChatCommandService.SkipToLevel(0);
		}

		Countdown.Start(_countdownTime, UpdateCountdownMessage, InvokeFinish);
	}

	public override IState GetNextState()
	{
		return new StateWaitingForHoF(StateMachine);
	}

	// Sent once per countdown second by UpdateCountdownMessage.
	private void SendServerMessage()
	{
		// Match.Winner instead of the winner of the last round: this state is also the final state of
		// the machine, so it is entered when the showdown is stopped at any point - including before a
		// single round exists, where Match.CurrentRound would throw.
		Team winnerTeam = Match?.Winner;
		if (winnerTeam == null)
		{
			// The showdown was stopped before a match was finished, so there is no winner to show.
			return;
		}

		// Decorated ServerMessage without emotes
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Match Over!", block => block.Bold().Color(ShowdownColors.Red))
			)
			.AddLine(line => line
				.AddBlock($"{winnerTeam.GetFullNameWithTag()} ", block => block.Color(winnerTeam.Color).Bold())
				.AddBlock("won the Match! :party:", block => block.Color(ShowdownColors.WinnerGold))
			)
			.AddLine(line => line
				.AddBlock("Final Score: ", block => block.Bold().Color(ShowdownColors.White))
				.AddBlock(Match.ScoreColored())
			)
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("You can now join us in Discord for an interview :smile:")
			)
			.AddLine(line => line
				.AddBlock("Teams will get kicked after")
				.AddBlock($"{_countdownTime}",
					block => block.Bold().Color(_countdownTime <= LowTimeThresholdSeconds
						? ShowdownColors.Red
						: ShowdownColors.Green))
				.AddBlock("seconds.")
			)
			.AddSeparator();

		msg.Send();
	}

	private void UpdateCountdownMessage(int remainingTime)
	{
		_countdownTime = remainingTime;
		SendServerMessage();
	}
}