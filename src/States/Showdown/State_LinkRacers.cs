using System;
using System.Linq;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Utils;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

public class StateLinkRacers(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private const int CountdownDuration = 3;

	// The (un)link commands are only available while racers are being linked to their teams.
	private readonly CommandLinkRacer _linkCommand = new();
	private readonly CommandUnLinkRacer _unlinkCommand = new();
	private bool _isCountdownRunning;

	private Team TeamA => Match.TeamA;
	private Team TeamB => Match.TeamB;

	public override void Enter()
	{
		CommandRegistry.RegisterMixed(_linkCommand);
		CommandRegistry.RegisterMixed(_unlinkCommand);

		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);

		AutoLinkPresentRacers();
		CheckIfRacersAreLinked();
	}

	public override void Exit()
	{
		CommandRegistry.Unregister(_linkCommand);
		CommandRegistry.Unregister(_unlinkCommand);
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

		AutoLinkPresentRacers();
		CheckIfRacersAreLinked();
	}

	public override void OnLinkRacer(ulong steamId, string arguments)
	{
		// Once the countdown to the next state is running, no more (un)linking is accepted.
		if (_isCountdownRunning)
		{
			return;
		}

		Team targetTeam = ResolveTeamFromArgument(arguments);
		if (targetTeam == null)
		{
			ChatMessage.SendCustomMessage(
				$"Usage: {ShowdownColors.Command("!link <1|2|TAG>")}, e.g. " +
				$"{ShowdownColors.Command("!link 1")}, " +
				$"{ShowdownColors.Command("!link #2")} or " +
				$"{ShowdownColors.Command($"!link {TeamA.Tag}")}.");
			return;
		}

		if (targetTeam.Racers.Count >= targetTeam.MaxTeamSize)
		{
			ChatMessage.SendCustomMessage($"Team {targetTeam.GetColoredTag()} is already full.");
			return;
		}

		// A player can only be part of one team - remove them from the other team first.
		TeamA.RemoveRacer(steamId);
		TeamB.RemoveRacer(steamId);

		string steamName = ZeepkistNetworkService.GetSteamNameFromSteamId(steamId);
		Racer racer = targetTeam.ExpectedRacers.FirstOrDefault(r => r.SteamId == steamId) ??
		              new Racer(steamId, steamName);
		targetTeam.AddRacer(racer); // Add racer to the requested team

		CheckIfRacersAreLinked(); // Check again after each link
	}

	// Handles '!unlink', which removes the calling player from whichever team they were on.
	// Example: a player types '!unlink' to leave their current team without joining another one.
	public override void OnUnlinkRacer(ulong steamId)
	{
		// Once the countdown to the next state is running, no more (un)linking is accepted.
		if (_isCountdownRunning)
		{
			return;
		}

		TeamA.RemoveRacer(steamId);
		TeamB.RemoveRacer(steamId);


		CheckIfRacersAreLinked();
	}

	// Index shown to players in front of each team's name (1-based) so '!link 1'/'!link 2' works.
	private static int GetTeamNumber(Team team, Team teamA, Team teamB)
	{
		return team == teamA ? 1 : 2;
	}

	// Teams.json already knows which SteamIds belong to a team. As soon as one of these players
	// is detected in the server, they are linked automatically without needing '!link'.
	private void AutoLinkPresentRacers()
	{
		foreach (Team team in new[] { TeamA, TeamB })
		foreach (Racer expectedRacer in team.ExpectedRacers)
		{
			bool isPresent = ZeepkistNetwork.PlayerList.Any(player => player.SteamID == expectedRacer.SteamId);
			if (isPresent)
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
					.AddBlock("'Select Initiative'", block => block.Color(ShowdownColors.Yellow))
					.AddBlock("in")
					.AddBlock($"{countdownTime}", block => block.Color(ShowdownColors.Green))
					.AddBlock("seconds...")
				);
		}

		msg.Send();
	}

	// Resolves the team targeted by '!link'. Accepts a 1-based team number (optionally prefixed
	// with '#', e.g. '1' / '#1') or the team tag (optionally wrapped in brackets, e.g. 'TAG' / '[TAG]').
	private Team ResolveTeamFromArgument(string arguments)
	{
		string argument = (arguments ?? string.Empty).Trim();
		if (argument.Length == 0)
		{
			return null;
		}

		if (argument.StartsWith("#"))
		{
			argument = argument[1..].Trim();
		}

		if (argument.StartsWith("[") && argument.EndsWith("]") && argument.Length >= 2)
		{
			argument = argument[1..^1].Trim();
		}

		if (int.TryParse(argument, out int teamNumber))
		{
			return teamNumber switch
			{
				1 => TeamA,
				2 => TeamB,
				_ => null
			};
		}

		if (string.Equals(TeamA.Tag, argument, StringComparison.OrdinalIgnoreCase))
		{
			return TeamA;
		}

		if (string.Equals(TeamB.Tag, argument, StringComparison.OrdinalIgnoreCase))
		{
			return TeamB;
		}

		return null;
	}

	private ServerMessage ServerMessageLinkedRacers()
	{
		ServerMessage msg = new ServerMessage()
				.ShowdownHeader()
				.AddLine(line => line
					.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
					.AddBlock("VS")
					.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color))
				)
				.AddSeparator()
				.AddLine(line => line
					.AddBlock("To join a team, type ")
					.AddBlock("'!link <number|TAG>'", format => format.Command())
					.AddBlock("in chat, e.g.")
					.AddBlock("'!link 1'", format => format.Command())
					.AddBlock("or")
					.AddBlock("'!link TAG'", format => format.Command())
				)
				.AddLine(line => line
					.AddBlock("To leave your team, type ")
					.AddBlock("'!unlink'", format => format.Command())
					.AddBlock("in chat")
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
				)
			;
		return msg;
	}
}