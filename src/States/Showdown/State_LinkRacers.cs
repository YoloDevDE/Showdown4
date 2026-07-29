using System.Linq;
using Showdown4.Entities;
using Showdown4.Utils;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

public class StateLinkRacers(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private const int CountdownDuration = 3;

	private bool _isCountdownRunning;

	public override void Enter()
	{
		ChatMessage.ClearChat();

		AutoLinkPresentRacers();
		CheckIfRacersAreLinked();
	}

	public override IState GetNextState()
	{
		return new StateTutorial(StateMachine);
	}

	public override void OnPlayerJoined(ZeepkistNetworkPlayer player)
	{
		if (_isCountdownRunning)
		{
			return;
		}

		LinkRacer(player.SteamID);
		CheckIfRacersAreLinked();
	}

	// Teams.json already knows which SteamIds belong to a team. As soon as one of these players
	// is detected in the server, they are automatically linked - players have no control over
	// which team they end up on, since that is entirely determined by the configured roster.
	private void AutoLinkPresentRacers()
	{
		foreach (ZeepkistNetworkPlayer player in ZeepkistNetwork.PlayerList) LinkRacer(player.SteamID);
	}

	// Links exactly one player (by SteamId) to their configured team, if they are expected there.
	private void LinkRacer(ulong steamId)
	{
		foreach (Team team in new[] { TeamA, TeamB })
		{
			Racer expectedRacer = team.ExpectedRacers.FirstOrDefault(racer => racer.SteamId == steamId);
			if (expectedRacer != null)
			{
				team.AddRacer(expectedRacer);
			}
		}
	}

	private void CheckIfRacersAreLinked()
	{
		if (TeamA.Racers.Count >= TeamA.MaxTeamSize && TeamB.Racers.Count >= TeamB.MaxTeamSize)
		{
			StartCountdown();
		}
		else
		{
			UpdateServerMessage(0);
		}
	}

	private void StartCountdown()
	{
		if (_isCountdownRunning)
		{
			return;
		}

		_isCountdownRunning = true;
		Countdown.Start(CountdownDuration, UpdateServerMessage, InvokeFinish);
	}

	private void UpdateServerMessage(int countdownTime)
	{
		// Create a consistent server message with appended countdown at the end
		ServerMessage msg = ServerMessageLinkedRacers();

		// Append the countdown timer if it's running
		if (_isCountdownRunning)
		{
			msg.AddSeparator()
				.AddLine(line => line
					.AddBlock("All Racers are linked to their Teams! ",
						block => block.Color(ShowdownColors.Green).Bold()))
				.AddSeparator()
				.AddLine(line => line
					.AddBlock("Continue to")
					.AddBlock("'Tutorial'", block => block.Color(ShowdownColors.Yellow))
					.AddBlock("in")
					.AddBlock($"{countdownTime}", block => block.Color(ShowdownColors.Green))
					.AddBlock("seconds...")
				);
		}

		msg.Send();
	}

	private ServerMessage ServerMessageLinkedRacers()
	{
		return new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
				.AddBlock("VS")
				.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color))
			)
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Waiting for all racers to join the server. Teams are assigned automatically ")
				.AddBlock("based on the configured roster.")
			)
			.AddSeparator()
			.AddLine("Members in each team:")
			.AddLine(line => line
				.AddBlock("1: ", format => format.Bold())
				.AddBlock($"{TeamA.GetColoredTag()} ", format => format.Color(TeamA.Color))
				.AddBlock($"{TeamA.GetLinkedRacersToString()}")
			)
			.AddLine(line => line
				.AddBlock("2: ", format => format.Bold())
				.AddBlock($"{TeamB.GetColoredTag()} ", format => format.Color(TeamB.Color))
				.AddBlock($"{TeamB.GetLinkedRacersToString()}")
			);
	}
}