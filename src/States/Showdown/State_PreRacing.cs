using System;
using System.Linq;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class StatePreRacing : IState
{
	private Team _teamA, _teamB;

	public StatePreRacing(IStateMachine stateMachine)
	{
		StateMachine = stateMachine;
	}

	private ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;

	public IStateMachine StateMachine { get; }

	public event Action Finished;

	public void Enter()
	{
		_teamA = Showdown.Match.TeamA;
		_teamB = Showdown.Match.TeamB;

		RacingApi.RoundEnded += OnRoundEnd;
		RacingApi.LevelLoaded += OnLevelLoaded;

		// Use the new CountdownTimer
		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
			10, // Countdown duration
			UpdateCountdownMessage, // Action on tick
			SkipToNextLevel // Action on completion
		));
	}

	public void Execute()
	{
		// Set the lobby time to 300 seconds (5 minutes)
		ChatApi.SendMessage("/settime 300");
	}

	public void Exit()
	{
		RacingApi.RoundEnded -= OnRoundEnd;
		RacingApi.LevelLoaded -= OnLevelLoaded;
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}

	private void OnLevelLoaded()
	{
		InvokeFinish();
	}

	// Update the server message with the countdown time
	private void UpdateCountdownMessage(int secondsRemaining)
	{
		var msg = new ServerMessage()
				.ShowdownHeader()
				.AddLine(line => line
					.AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
					.AddBlock("VS")
					.AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
				)
				.AddSeparator()
				.AddMessage(ShowPickedMaps())
				.AddSeparator()
				.AddLine(line => line
						.AddBlock("Race starts in:")
						.AddBlock($"{secondsRemaining} seconds", f => f.Bold().Color("#ff0000")) // Red countdown
				)
			;

		msg.Send();
	}

	private void SkipToNextLevel()
	{
		ChatApi.SendMessage("/fs 0"); // Move to the next level
	}

	private ServerMessage ShowPickedMaps()
	{
		var msg = new ServerMessage()
			.AddLine(line => line
				.AddBlock("Picked Maps:"));

		var round = 1;
		foreach (var pickedLevel in Showdown.Match.Drafts.SelectMany(matchDraft => matchDraft.PickedLevels))
			msg.AddLine(line =>
			{
				if (round <= Showdown.Match.RoundCounter()) line.StrikeThrough();

				line
					.AddBlock($"Round {round}:")
					.AddBlock($"'{pickedLevel.Level.Name}'", block => block.Color("#00ffff"))
					.AddBlock("picked by", block => block.Indent("585%"))
					.AddBlock($"{pickedLevel.Team.GetColoredTag()}")
					.Bold();
				round++;
			});

		return msg;
	}

	private void OnRoundEnd()
	{
		var nextLevel = Showdown.Match.CurrentDraft.PickedLevels.First();
		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder().ClearChat()
				.DashedLine().NewLine()
				.TextLine(
					$"Starting <b>{(Showdown.Match.RoundCounter() + 1 == 3 ? "Tiebreaker" : $"Round {Showdown.Match.RoundCounter() + 1}")}</b>")
				.NewLine()
				.DashedLine().NewLine()
				.TextLine($"Level <color=#00ffff>'{nextLevel.Level.Name}'</color>").NewLine()
				.TextLine($"picked by {nextLevel.Team.GetColoredTag()}").NewLine()
				.DashedLine().NewLine()
				.TextLine($"{Showdown.Match.Score()}")
				.Build().Message
		);
	}
}