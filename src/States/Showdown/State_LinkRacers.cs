using System;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

public class StateLinkRacers : IState
{
	private const int CountdownDuration = 3;
	private Team _currentTeam;
	private bool _isCountdownRunning;

	public StateLinkRacers(IStateMachine stateMachine)
	{
		StateMachine = stateMachine;
	}

	private Team TeamA => Showdown.Match.TeamA;
	private Team TeamB => Showdown.Match.TeamB;

	private ShowdownStateMachine Showdown => StateMachine as ShowdownStateMachine;

	public IStateMachine StateMachine { get; }

	public event Action Finished;

	public void Enter()
	{
		CommandLinkRacer.CommandInvoked += OnLinkRacerToTeam;
		CommandUnLinkRacer.CommandInvoked += OnUnLinkRacerToTeam;
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
	}

	public void Execute()
	{
		_currentTeam = TeamA;
		CheckIfRacersAreLinked();
	}

	public void Exit()
	{
		CommandLinkRacer.CommandInvoked -= OnLinkRacerToTeam;
		CommandUnLinkRacer.CommandInvoked -= OnUnLinkRacerToTeam;
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}

	private void CheckIfRacersAreLinked()
	{
		if (TeamA.Racers.Count >= TeamA.MaxTeamSize && TeamB.Racers.Count >= TeamB.MaxTeamSize)
		{
			CommandLinkRacer.CommandInvoked -= OnLinkRacerToTeam;
			CommandUnLinkRacer.CommandInvoked -= OnUnLinkRacerToTeam;
			StartCountdown();
		}
		else
		{
			_currentTeam = TeamA.Racers.Count < TeamA.MaxTeamSize ? TeamA : TeamB;
			UpdateServerMessage(0);
		}
	}

	private void StartCountdown()
	{
		if (!_isCountdownRunning)
		{
			_isCountdownRunning = true;
			CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
				CountdownDuration,
				UpdateServerMessage,
				InvokeFinish // Move to next state when countdown finishes
			));
		}
	}

	private void UpdateServerMessage(int countdownTime)
	{
		// Create a consistent server message with appended countdown at the end
		var msg = ServerMessageLinkedRacers();


		// Append the countdown timer if it's running
		if (_isCountdownRunning)
			msg.AddSeparator()
				.AddLine(line => line
					.AddBlock("All Racers are linked to their Teams! ", block => block.Color("#00ff00").Bold()))
				.AddSeparator()
				.AddLine(line => line
					.AddBlock("Continue to")
					.AddBlock("'Select Initiative'", block => block.Color("#ffff00"))
					.AddBlock("in")
					.AddBlock($"{countdownTime}", block => block.Color("#00ff00"))
					.AddBlock("seconds...")
				);

		msg.Send();
	}

	private void OnLinkRacerToTeam(ulong steamId)
	{
		var steamName = ZeepkistNetworkService.GetSteamNameFromSteamId(steamId);
		var racer = new Racer(steamId, steamName);
		_currentTeam.AddRacer(racer); // Add racer to current team


		CheckIfRacersAreLinked(); // Check again after each link
	}

	private void OnUnLinkRacerToTeam(ulong steamId)
	{
		TeamA.RemoveRacer(steamId);
		TeamB.RemoveRacer(steamId);


		CheckIfRacersAreLinked();
	}

	private ServerMessage ServerMessageLinkedRacers()
	{
		var msg = new ServerMessage()
				.ShowdownHeader()
				.AddLine(line => line
					.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
					.AddBlock("VS")
					.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color))
				)
				.AddSeparator()
				.AddLine(line => line
					.AddBlock("To join team ")
					.AddBlock($"{_currentTeam.GetColoredTag()}", format => format.Color($"{_currentTeam.Color}").Bold())
					.AddBlock(", type ")
					.AddBlock("'!link'", format => format.Color("#ffff00").Bold())
					.AddBlock(" in chat")
				)
				.AddSeparator()
				.AddLine("Members in each team:")
				.AddLine(line => line
					.AddBlock($"{TeamA.GetColoredTag()} ", format => format.Color(TeamA.Color))
					.AddBlock($"{TeamA.GetLinkedRacersToString()}")
				)
				.AddLine(line => line
					.AddBlock($"{TeamB.GetColoredTag()} ", format => format.Color(TeamB.Color))
					.AddBlock($"{TeamB.GetLinkedRacersToString()}")
				)
			;
		return msg;
	}
}