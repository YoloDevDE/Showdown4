using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

public class StatePreRacing(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private Team _teamA, _teamB;

	public override void Enter()
	{
		_teamA = Match.TeamA;
		_teamB = Match.TeamB;

		// Use the new CountdownTimer
		CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
			MyConfig.PreRaceCountdownConfig.Value, // Countdown duration
			UpdateCountdownMessage, // Action on tick
			SkipToNextLevel // Action on completion
		));

		// Set the lobby time (in seconds)
		ChatCommandService.SetTime(MyConfig.LobbyTimeConfig.Value);
	}

	public override void Exit()
	{
	}

	public override IState GetNextState()
	{
		return new StateRacing(StateMachine);
	}

	public override void OnLevelLoaded()
	{
		InvokeFinish();
	}

	public override void OnRoundEnded()
	{
		DraftAction nextLevel = Match.CurrentDraft.PickedLevels[0];
		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder().ClearChat()
				.DashedLine().NewLine()
				.TextLine(
					$"Starting <b>{(Match.RoundCounter() + 1 == 3 ? "Tiebreaker" : $"Round {Match.RoundCounter() + 1}")}</b>")
				.NewLine()
				.DashedLine().NewLine()
				.TextLine($"Level <{ShowdownColors.Cyan}>'{nextLevel.Level.Name}'</color>").NewLine()
				.TextLine($"picked by {nextLevel.Team.GetColoredTag()}").NewLine()
				.DashedLine().NewLine()
				.TextLine($"{Match.Score()}")
				.Build().Message
		);
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
		ChatCommandService.SkipToLevel(0); // Move to the next level
	}
}