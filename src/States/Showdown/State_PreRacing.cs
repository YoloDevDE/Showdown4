using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using ZeepSDK.Chat;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class StatePreRacing : ShowdownStateBase
{
	private Team _teamA, _teamB;

	public StatePreRacing(IStateMachine stateMachine) : base(stateMachine)
	{
	}

	public override void Enter()
	{
		_teamA = Match.TeamA;
		_teamB = Match.TeamB;

		RacingApi.RoundEnded += OnRoundEnd;
		RacingApi.LevelLoaded += OnLevelLoaded;

		// Use the new CountdownTimer
		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
			MyConfig.Validated.PreRaceCountdown, // Countdown duration
			UpdateCountdownMessage, // Action on tick
			SkipToNextLevel // Action on completion
		));
	}

	public override void Execute()
	{
		// Set the lobby time (in seconds)
		ChatApi.SendMessage($"/settime {MyConfig.Validated.LobbyTime}");
	}

	public override void Exit()
	{
		RacingApi.RoundEnded -= OnRoundEnd;
		RacingApi.LevelLoaded -= OnLevelLoaded;
	}

	private void OnLevelLoaded()
	{
		InvokeFinish();
	}

	// Update the server message with the countdown time
	private void UpdateCountdownMessage(int secondsRemaining)
	{
		ServerMessage msg = new ServerMessage()
				.ShowdownHeader()
				.AddLine(line => line
					.AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
					.AddBlock("VS")
					.AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
				)
				.AddSeparator()
				.AddMessage(new ServerMessage().AppendPickedMaps(Match))
				.AddSeparator()
				.AddLine(line => line
						.AddBlock("Race starts in:")
						.AddBlock($"{secondsRemaining} seconds",
							f => f.Bold().Color(ShowdownColors.Red)) // Red countdown
				)
			;

		msg.Send();
	}

	private void SkipToNextLevel()
	{
		ChatApi.SendMessage("/fs 0"); // Move to the next level
	}

	private void OnRoundEnd()
	{
		DraftAction nextLevel = Match.CurrentDraft.PickedLevels[0];
		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder().ClearChat()
				.DashedLine().NewLine()
				.TextLine(
					$"Starting <b>{(Match.RoundCounter() + 1 == 3 ? "Tiebreaker" : $"Round {Match.RoundCounter() + 1}")}</b>")
				.NewLine()
				.DashedLine().NewLine()
				.TextLine($"Level <color={ShowdownColors.Cyan}>'{nextLevel.Level.Name}'</color>").NewLine()
				.TextLine($"picked by {nextLevel.Team.GetColoredTag()}").NewLine()
				.DashedLine().NewLine()
				.TextLine($"{Match.Score()}")
				.Build().Message
		);
	}
}