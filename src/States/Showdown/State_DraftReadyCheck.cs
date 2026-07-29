using System;
using System.Collections.Generic;
using Showdown4.Commands;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

/// <summary>
///     Ready check that runs before the very first draft (Draftphase I) only, right after
///     <see cref="StateSelectInitiative" /> determined who drafts first. It shows how the draft works
///     (pick/ban/pass) and who has initiative, so players know what to expect before the draft intro
///     of <see cref="StatePreDraft" /> plays. Draftphase II skips this state entirely.
/// </summary>
public class StateDraftReadyCheck(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// The ready command is only available during the ready check.
	private readonly CommandReady _readyCommand = new();
	private int _elapsedTicks;
	private HashSet<ulong> _readyPlayers;
	private int _remainingTime;
	private bool _stopped;

	private Team InitiativeTeam => Match.Initiative;

	public override void Enter()
	{
		CommandRegistry.RegisterMixed(_readyCommand);

		_readyPlayers = new HashSet<ulong>();
		_stopped = false;
		ChatMessage.ClearChat();

		Countdown.Start(MyConfig.ReadyCheckDurationConfig.Value, OnTick, OnTimeout);
	}

	public override IState GetNextState()
	{
		return new StatePreDraft(StateMachine);
	}

	public override void Exit()
	{
		CommandRegistry.Unregister(_readyCommand);
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
		_elapsedTicks++;
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
		AppendTutorialTip(msg);

		msg.Send();
	}

	// The first time a session sees the tutorial, it plays as its own full sequence. Every match
	// after that reuses the exact same content here, rotating one topic at a time - like tips on a
	// loading screen - instead of showing the full tutorial again.
	private void AppendTutorialTip(ServerMessage msg)
	{
		if (!StateTutorial.HasPlayedOnce || TutorialContent.Pages.Count == 0)
		{
			return;
		}

		int stepDuration = Mathf.Max(MyConfig.TutorialStepDurationConfig.Value, 1);
		int pageIndex = _elapsedTicks / stepDuration % TutorialContent.Pages.Count;
		TutorialContent.Page page = TutorialContent.Pages[pageIndex];

		msg.AddSeparator()
			.AddLine(line => line
				.AddBlock("Tip:", block => block.Color(ShowdownColors.Gold).Bold())
				.AddBlock(page.Title, block => block.Color(ShowdownColors.Yellow).Bold()));

		foreach (Action<ServerMessage.LineBuilder> line in page.Lines) msg.AddLine(line);
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
		_stopped = true; // The ready-check countdown is over, the ready-confirm countdown takes over
		ChatMessage.SendCustomMessage("All players are ready! Let the draft begin!");

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

		// Restarting the countdown implicitly replaces the ready-check timer above.
		Countdown.Start(MyConfig.ReadyConfirmCountdownConfig.Value, onComplete: () =>
		{
			ChatMessage.ClearChat();
			InvokeFinish();
		});
	}
}