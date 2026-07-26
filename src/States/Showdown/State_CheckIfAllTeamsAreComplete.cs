using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

/// <summary>
///     Gate that runs after the matchup has been selected (<see cref="StateSetupMatch" />) to make
///     sure both selected teams (<see cref="Match.TeamA" />/<see cref="Match.TeamB" />) actually
///     have a full, configured roster - either directly in the BepInEx config or in the legacy
///     Teams.json fallback (loaded through ZeepSDK's IModStorage, see <see cref="TeamsProvider" />).
///     As long as that is not the case we simply wait and keep rechecking every few seconds instead
///     of letting the racer linking phase crash or stall on an incomplete team.
/// </summary>
public class StateCheckIfAllTeamsAreComplete(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private const int RecheckIntervalSeconds = 5;

	private Team TeamA => Match.TeamA;
	private Team TeamB => Match.TeamB;

	public override void Enter()
	{
		CheckTeams();
	}

	public override IState GetNextState()
	{
		return new StateLinkRacers(StateMachine);
	}

	private void CheckTeams()
	{
		if (TeamsProvider.AreTeamsComplete(new List<Team> { TeamA, TeamB }))
		{
			InvokeFinish();
			return;
		}

		UpdateCountdownMessage(RecheckIntervalSeconds);
		Countdown.Start(RecheckIntervalSeconds, UpdateCountdownMessage, CheckTeams);
	}

	private void UpdateCountdownMessage(int secondsRemaining)
	{
		ServerMessage msg = new ServerMessage()
				.ShowdownHeader()
				.AddLine(line => line
					.AddBlock("Waiting for teams")
				)
				.AddSeparator()
				.AddLine(line => line
					.AddBlock($"{TeamA.GetColoredTag()}")
					.AddBlock("and")
					.AddBlock($"{TeamB.GetColoredTag()}")
					.AddBlock("to be fully configured")
				)
				.AddSeparator()
				.AddLine(line => line
					.AddBlock("Checking again in:")
					.AddBlock($"{secondsRemaining} seconds",
						f => f.Bold().Color(ShowdownColors.Red))
				)
			;

		msg.Send();
	}
}