using System;
using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

public class StateSelectInitiative : IState
{
	private const int CountdownDuration = 2; // Countdown in seconds
	private int _currentSelectionIndex; // To track the currently selected team
	private bool _isLocked; // To lock input after selecting initiative
	private Team _selectedTeam; // The team with initiative
	private List<Team> _teams;

	public StateSelectInitiative(IStateMachine stateMachine)
	{
		StateMachine = stateMachine;
	}

	private Team TeamA => Showdown.Match.TeamA;
	private Team TeamB => Showdown.Match.TeamB;

	private ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;

	public IStateMachine StateMachine { get; }

	public event Action Finished;

	public void Enter()
	{
		// Initialize the teams and reset selection
		_teams = new List<Team> { Showdown.Match.TeamA, Showdown.Match.TeamB };
		_currentSelectionIndex = 0;
		_selectedTeam = null;
		_isLocked = false; // Allow input at the start

		// Register this state for input detection
		StateManager.Instance.SetCurrentState(this);
	}

	public void Execute()
	{
		ServerMessageThing().Send();
	}

	public void Exit()
	{
		// Unregister this state from input detection
		StateManager.Instance.SetCurrentState(null);
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}

	public void HandleInput()
	{
		if (_isLocked)
			// Do nothing if input is locked
			return;

		var inputDetected = false;

		// Detect arrow key presses
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
		{
			// Toggle between two teams using the arrow keys
			_currentSelectionIndex = (_currentSelectionIndex + 1) % _teams.Count;
			inputDetected = true;
		}

		// Confirm selection with Space
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SelectTeamWithInitiative(); // Confirm the selection
			inputDetected = true;
		}

		// Update the display only if there was input
		if (inputDetected) Execute();
	}

	private ServerMessage ServerMessageThing()
	{
		// Display the current team selection in the server message
		var serverMessage = new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
				.AddBlock("VS")
				.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color))
			)
			.AddSeparator()
			.AddLine(line => line.AddBlock("Selecting Initiative (Press SPACE to confirm)"))
			.AddSeparator();

		for (var index = 0; index < _teams.Count; index++)
		{
			var team = _teams[index];

			if (index == _currentSelectionIndex)
				// Highlight the currently selected team
				serverMessage.AddLine(line => line
					.AddBlock($"> {index}: ", block => block.Color("#ffff00"))
					.AddBlock(team.GetNameWithTag(), block => block.Color(team.Color))
					.Bold());
			else
				// Show the other team normally
				serverMessage.AddLine(line => line
					.AddBlock($"{index}: ")
					.AddBlock(team.GetNameWithTag(), block => block.Color(team.Color))
				);
		}

		return serverMessage;
	}

	private void SelectTeamWithInitiative()
	{
		// Confirm the selected team for initiative
		_selectedTeam = _teams[_currentSelectionIndex];
		Showdown.Match.Initiative = _selectedTeam;

		// Lock the input after the selection
		_isLocked = true;

		// Start the countdown using CountdownTimer
		CoroutineManager.Instance.StartExternalCoroutine(
			CountdownTimer.Start(CountdownDuration, UpdateCountdownMessage, Finished)
		);
	}

	private void UpdateCountdownMessage(int countdownTime)
	{
		// Send or append the countdown message to the server
		var msg =
			ServerMessageThing()
				.AddSeparator()
				.AddLine(line => line
					.AddBlock("Initiative has been given to")
				)
				.AddLine(line => line
					.AddBlock(_selectedTeam.GetNameWithTag(), f => f.Color(_selectedTeam.Color).Bold()));
		msg.Send();
	}
}