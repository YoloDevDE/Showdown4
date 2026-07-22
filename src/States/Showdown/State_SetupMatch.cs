using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

public class StateSetupMatch(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private const int CountdownDuration = 2; // Countdown in seconds
	private int _currentSelectionIndex;
	private bool _isCountdownActive;
	private int _remainingSeconds;
	private Team _selectedTeamA;
	private Team _selectedTeamB;
	private List<Team> _teams;

	public override void Enter()
	{
		TeamData teamData = Plugin.Storage.LoadFromJson<TeamData>(MyConfig.TeamFileConfig.Value);
		_teams = teamData?.Teams ?? new List<Team>();

		_currentSelectionIndex = 0;
		_selectedTeamA = null;
		_selectedTeamB = null;
		_isCountdownActive = false;

		ChatCommandService.SetTime(86400);
		if (_teams.Count == 0)
		{
			ChatMessage.SendCustomMessage("No teams found in the JSON file.");
		}

		StateManager.Instance.SetCurrentState(this);
		SendServerMessage();
	}

	public override void Exit()
	{
		StateManager.Instance.SetCurrentState(null);
	}

	public override void HandleInput()
	{
		bool inputDetected = false;

		if (!_isCountdownActive)
		{
			// Detect arrow key presses
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				_currentSelectionIndex = (_currentSelectionIndex - 1 + _teams.Count) % _teams.Count; // Move up
				inputDetected = true;
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				_currentSelectionIndex = (_currentSelectionIndex + 1) % _teams.Count; // Move down
				inputDetected = true;
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				SelectTeam(); // Confirm team selection
				inputDetected = true;
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				ResetSelection(); // Reset selection
				inputDetected = true;
			}
		}

		// Start countdown if space is pressed and teams are selected
		if (Input.GetKeyDown(KeyCode.Space) && _selectedTeamA != null && _selectedTeamB != null && !_isCountdownActive)
		{
			ConfirmTeams();
			StartCountdown();
		}

		if (inputDetected)
		{
			SendServerMessage();
		}
	}

	private void SendServerMessage()
	{
		// ServerMessage showing the team selection
		ServerMessage serverMessage = new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line.AddBlock("Setup Match",
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold()
					.AllCaps().Size(40)))
			.AddSeparator();

		for (int i = 0; i < _teams.Count; i++)
		{
			Team team = _teams[i];
			int index = i;

			if (i == _currentSelectionIndex && (team == _selectedTeamA || team == _selectedTeamB))
			{
				serverMessage.AddLine(line => line
					.AddBlock($"{(team == _selectedTeamA ? "A" : "B")} > {index}: ",
						block => block.Color(ShowdownColors.Green))
					.AddBlock(team.GetNameWithTag(), block => block.Color(team.Color))
					.Bold().Underline());
			}
			else if (team == _selectedTeamA || team == _selectedTeamB)
			{
				serverMessage.AddLine(line => line
					.AddBlock($"{(team == _selectedTeamA ? "A" : "B")}   {index}: ")
					.AddBlock(team.GetNameWithTag(), block => block.Color(team.Color))
					.Bold().Underline());
			}
			else if (i == _currentSelectionIndex)
			{
				serverMessage.AddLine(line => line
					.AddBlock($"  > {index}: ", block => block.Color(ShowdownColors.Yellow))
					.AddBlock(team.GetNameWithTag(), block => block.Color(team.Color))
					.Bold());
			}
			else
			{
				serverMessage.AddLine(line => line
					.AddBlock($"    {index}: ")
					.AddBlock(team.GetNameWithTag(), block => block.Color(team.Color)));
			}
		}

		// Display the countdown if it's active
		if (_isCountdownActive)
		{
			serverMessage.AddSeparator()
				.AddLine(line => line
					.AddBlock("Continue to")
					.AddBlock("'Link Racers'", block => block.Color(ShowdownColors.Yellow))
					.AddBlock("in")
					.AddBlock($"{_remainingSeconds}", block => block.Color(ShowdownColors.Green))
					.AddBlock("seconds..."));
		}

		serverMessage.Send();
	}

	private void SelectTeam()
	{
		Team selectedTeam = _teams[_currentSelectionIndex];

		if (_selectedTeamA == null)
		{
			_selectedTeamA = selectedTeam;
		}
		else if (_selectedTeamB == null && selectedTeam != _selectedTeamA)
		{
			_selectedTeamB = selectedTeam;
			ChatMessage.SendCustomMessage(
				$"Teams selected:<br>{_selectedTeamA.GetColoredTag()} vs {_selectedTeamB.GetColoredTag()}.<br>Press space to confirm.");
		}
	}

	private void ResetSelection()
	{
		_selectedTeamA = null;
		_selectedTeamB = null;
		ChatMessage.SendCustomMessage("Team selections have been reset.");
	}

	private void StartCountdown()
	{
		if (_selectedTeamA != null && _selectedTeamB != null)
		{
			_isCountdownActive = true;

			// Start the countdown using CountdownTimer.Start
			CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
				CountdownDuration, // Duration of the countdown
				remainingSeconds =>
				{
					_remainingSeconds = remainingSeconds;
					SendServerMessage(); // Action to update message during countdown
				},
				InvokeFinish // Action when countdown completes
			));
		}
	}

	private void ConfirmTeams()
	{
		ChatMessage.SendCustomMessage(
			$"Teams confirmed:<br>{_selectedTeamA.GetFullColoredTagAndName()} vs {_selectedTeamB.GetFullColoredTagAndName()}.");
		Showdown.Match = new Match(_selectedTeamA, _selectedTeamB);
	}
}