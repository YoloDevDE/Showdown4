using System.Collections;
using System.Collections.Generic;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

public class StateReadyCheck : ShowdownStateBase
{
	private const int ReadyCheckDuration = 300; // Countdown in seconds
	private HashSet<ulong> _readyPlayers; // Store the IDs of players who are ready
	private int _remainingTime; // Countdown in seconds

	public StateReadyCheck(IStateMachine stateMachine) : base(stateMachine)
	{
	}

	private Team TeamA => Match.TeamA;
	private Team TeamB => Match.TeamB;

	public override void Enter()
	{
		_readyPlayers = new HashSet<ulong>();
		_remainingTime = ReadyCheckDuration;
		CommandReady.CommandInvoked += OnReady;
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);

		// Start the timer coroutine
		CoroutineManager.Instance.StartExternalCoroutine(TimerCoroutine());
	}

	public override void Execute()
	{
		ServerMessage msg = ShowPickedMaps()
			.AddSeparator()
			.AddInLine(line => line
				.AddBlock("Remaining Time:")
				.AddBlock($"{TimeFormatter.FormatDuration(_remainingTime)}", block => block.Color(ShowdownColors.Red))
			)
			.AddLine(line => line.AddBlock("Ready Status:"));

		foreach (KeyValuePair<uint, ZeepkistNetworkPlayer> playerEntry in ZeepkistNetwork.Players)
		{
			ZeepkistNetworkPlayer player = playerEntry.Value;
			bool isReady = _readyPlayers.Contains(player.SteamID);
			msg.AddLine(line => line
				.AddBlock($"{player.Username}:")
				.AddBlock(isReady ? "Ready" : "Not Ready",
					block => block.Color(isReady ? ShowdownColors.Green : ShowdownColors.Red))
			);
		}

		msg.Send();
	}

	public override void Exit()
	{
		CommandReady.CommandInvoked -= OnReady;
		_remainingTime = 0; // This will cause the timer coroutine to stop
	}

	private IEnumerator TimerCoroutine()
	{
		while (_remainingTime > 0)
		{
			Execute(); // Update the display every second
			yield return new WaitForSeconds(1);
			_remainingTime--;

			// Check if time ran out
			if (_remainingTime <= 0)
			{
				ChatMessage.SendCustomMessage("Ready check timed out!");
				InvokeFinish();
				yield break;
			}
		}
	}

	private void OnReady(ulong steamId, string arg)
	{
		if (!_readyPlayers.Contains(steamId))
		{
			_readyPlayers.Add(steamId); // Mark player as ready


			// Check if all players are ready
			if (_readyPlayers.Count >= ZeepkistNetwork.Players.Count)
			{
				ConfirmReady(); // All players ready, finish the state
			}
			else
			{
				Execute(); // Update the ready status in the server message
			}
		}
	}

	private ServerMessage ShowPickedMaps()
	{
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
				.AddBlock("VS")
				.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color))
			)
			.AddSeparator()
			.AppendPickedMaps(Match)
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("To ready up, type")
				.AddBlock("'!ready'", b => b.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("in the chat.")
			);

		return msg;
	}

	private void ConfirmReady()
	{
		_remainingTime = 0; // Stop the timer
		// Notify that all players are ready
		ChatMessage.SendCustomMessage("All players are ready! GL HF");
		// Display message in the server with a 2-second delay before proceeding
		CoroutineManager.Instance.StartExternalCoroutine(ReadyCountdown());
	}

	private IEnumerator ReadyCountdown()
	{
		ServerMessage msg = ShowPickedMaps()
			.AddSeparator()
			.AddLine(line => line.AddBlock("Ready Status:"));

		foreach (KeyValuePair<uint, ZeepkistNetworkPlayer> playerEntry in ZeepkistNetwork.Players)
		{
			ZeepkistNetworkPlayer player = playerEntry.Value;
			bool isReady = _readyPlayers.Contains(player.SteamID);
			msg.AddLine(line => line
				.AddBlock($"{player.Username}: ", block => block.Bold())
				.AddBlock(isReady ? "Ready" : "Not Ready",
					block => block.Color(isReady ? ShowdownColors.Green : ShowdownColors.Red))
			);
		}

		msg
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Everyone ready. Prepare for battle!", block => block.Color(ShowdownColors.Green).Bold()))
			.AddSeparator();
		msg.Send();

		// Wait for 2 seconds before transitioning to the next state
		yield return new WaitForSeconds(3);
		// Notify that all players are ready
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
		ChatMessage.SendCustomMessage($"Racing starts in <color={ShowdownColors.Red}>10</color> seconds");
		// Proceed to the next state
		InvokeFinish();
	}
}