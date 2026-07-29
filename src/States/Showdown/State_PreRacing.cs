using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

public class StatePreRacing(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		Countdown.Start(MyConfig.PreRaceCountdownConfig.Value, UpdateCountdownMessage, SkipToNextLevel);

		// Set the lobby time (in seconds)
		ChatCommandService.SetTime(MyConfig.LobbyTimeConfig.Value);
	}

	public override IState GetNextState()
	{
		return new StateSynchro(StateMachine);
	}

	public override void OnLevelLoaded()
	{
		InvokeFinish();
	}

	public override void OnRoundEnded()
	{
		DraftAction nextLevel = Match.GetUpcomingLevel();
		if (nextLevel == null)
		{
			return;
		}

		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder().ClearChat()
				.DashedLine().NewLine()
				.TextLine($"Starting <b>{Match.UpcomingRoundName()}</b>")
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
		new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
				.AddBlock("VS")
				.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color))
			)
			.AddSeparator()
			.AddMessage(new ServerMessage().AppendPickedMaps(Match))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Race starts in:")
				.AddBlock($"{secondsRemaining} seconds", f => f.Bold().Color(ShowdownColors.Red))
			)
			.Send();
	}

	// Loads the level of the upcoming round. StateDraftCompleted locked the drafted levels into the
	// server playlist in exactly the order they were picked, so the draft index is the playlist index
	// - a hardcoded 0 here would replay the first drafted map in every following round.
	private void SkipToNextLevel()
	{
		int levelIndex = Match.GetUpcomingLevelIndex();
		ChatCommandService.SkipToLevel(levelIndex < 0 ? 0 : levelIndex);
	}
}