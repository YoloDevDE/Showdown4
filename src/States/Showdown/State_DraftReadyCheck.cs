using System.Collections;
using System.Collections.Generic;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

// Ready check that runs before the very first draft (Draftphase I) only, right after
// StatePreDraft announced initiative. It shows how the draft works (pick/ban/pass) and who has
// initiative, so players know what to expect before the draft actually starts. Once everyone is
// ready, a short countdown plays and StateDrafting begins. Draftphase II skips this state
// entirely (see StatePreDraft / StatePostRacing).
public class StateDraftReadyCheck(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private HashSet<ulong> _readyPlayers;
	private int _remainingTime;
	private bool _stopped;

	private Team TeamA => Match.TeamA;
	private Team TeamB => Match.TeamB;
	private Team InitiativeTeam => Match.Initiative;

	public override void Enter()
	{
		_readyPlayers = new HashSet<ulong>();
		_stopped = false;
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);

		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
			MyConfig.ReadyCheckDurationConfig.Value,
			OnTick,
			OnTimeout));
	}

	public override IState GetNextState()
	{
		return new StatePreDraft(stateMachine);
	}

	public override void Exit()
	{
		_stopped = true; // Prevents a still-running countdown from acting once we left this state
	}

	public override void OnReady(ulong steamId, string arguments)
	{
		if (!_readyPlayers.Add(steamId))
		{
			return;
		}

		if (_readyPlayers.Count >= ZeepkistNetwork.Players.Count)
		{
			ConfirmReady();
		}
		else
		{
			SendServerMessage();
		}
	}

	private void OnTick(int remainingSeconds)
	{
		if (_stopped)
		{
			return;
		}

		_remainingTime = remainingSeconds;
		SendServerMessage();
	}

	private void OnTimeout()
	{
		if (_stopped)
		{
			return;
		}

		ChatMessage.SendCustomMessage("Ready check timed out!");
		InvokeFinish();
	}

	private void SendServerMessage()
	{
		ServerMessage msg = ExplainDraftMessage()
			.AddSeparator()
			.AddInLine(line => line
				.AddBlock("Remaining Time:")
				.AddBlock($"{TimeFormatter.FormatDuration(_remainingTime)}", block => block.Color(ShowdownColors.Red))
			)
			.AddLine(line => line.AddBlock("Ready Status:"));

		AppendReadyStatus(msg);

		msg.Send();
	}

	private ServerMessage ExplainDraftMessage()
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
				.AddBlock("INITIATIVE:", block => block.Color(ShowdownColors.Gold).Bold()))
			.AddLine(line => line
				.AddBlock($"{InitiativeTeam.GetNameWithTag()}", b => b.Color(InitiativeTeam.Color).Bold().Size(30))
				.AddBlock("drafts first!"))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("How the draft works", builder => builder.Color(ShowdownColors.Gold).Bold()))
			.AddLine(line => line
				.AddBlock("!ban 1-7", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("- remove a map"))
			.AddLine(line => line
				.AddBlock("!pick 1-7", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("- choose a map to play"))
			.AddLine(line => line
				.AddBlock("!pass", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("- give your action to the other team"))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("To ready up, type")
				.AddBlock("'!ready'", b => b.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("in the chat."));
	}

	private void AppendReadyStatus(ServerMessage msg)
	{
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
	}

	private void ConfirmReady()
	{
		_stopped = true; // Stops the ready-check countdown, the ready countdown below takes over
		ChatMessage.SendCustomMessage("All players are ready! Let the draft begin!");
		CoroutineManager.Instance.StartExternalCoroutine(ReadyCountdown());
	}

	private IEnumerator ReadyCountdown()
	{
		ServerMessage msg = ExplainDraftMessage()
			.AddSeparator()
			.AddLine(line => line.AddBlock("Ready Status:"));

		AppendReadyStatus(msg);

		msg
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Everyone ready. The draft is about to start!",
					block => block.Color(ShowdownColors.Green).Bold()))
			.AddSeparator();
		msg.Send();

		yield return new WaitForSeconds(MyConfig.ReadyConfirmCountdownConfig.Value);
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);

		InvokeFinish();
	}
}