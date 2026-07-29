using Showdown4.Config;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

/// <summary>
///     Short phase right after a map was loaded: everyone is wiped from the leaderboard and respawned
///     at the same moment, so the warm up starts perfectly synchronized for all racers.
/// </summary>
public class StateSynchro(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		// Nobody sets a time yet - the warm up state keeps everyone blocked as well.
		RacerResetService.BlockEveryoneFromSettingTime();

		// Arm the "+24h" lobby timer right here so it can never run out (and thus never switch the
		// map on its own) during synchro, warm up or any of the following sub-rounds. The in-game
		// timer only displays up to 24h, so everyone still sees the intended race duration.
		ChatCommandService.SetRaceTime(MyConfig.RacingDurationConfig.Value);

		Countdown.Start(MyConfig.SynchroSecondsConfig.Value, UpdateCountdownMessage, OnCountdownComplete);
	}

	public override IState GetNextState()
	{
		return new StateWarmUp(StateMachine);
	}

	// The whole point of the synchro phase: after those few seconds everyone is wiped from the
	// leaderboard and respawned, so the warm up round starts for all racers at the very same moment.
	private void OnCountdownComplete()
	{
		RacerResetService.ClearLeaderboardForEveryone();
		RacerResetService.RespawnEveryone();
		InvokeFinish();
	}

	private void UpdateCountdownMessage(int secondsRemaining)
	{
		if (secondsRemaining <= 0)
		{
			return;
		}

		new ServerMessage("center")
			.ShowdownHeader(false, "center")
			.AddSeparator()
			.AddLine(line => line.AddBlock("Synchronizing Players...", b => b.Color(ShowdownColors.Cyan)))
			.AddLine(line => line.AddBlock($"Starting in {secondsRemaining}s", b => b.Color(ShowdownColors.Gray)))
			.Send();
	}
}