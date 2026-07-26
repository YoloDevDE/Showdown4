using Showdown4.Entities;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

internal class StatePostRacing(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		Team winnerTeam = Match.CurrentRound.GetWinnerTeam;
		winnerTeam.AddWin();

		// Get the current round index
		int currentRoundCounter = Match.RoundCounter();

		// Default message in case of an error
		string nextLevelMessage = "";
		// Check if the current round index is within the range of picked levels
		if (currentRoundCounter < Match.CurrentDraft.PickedLevels.Count)
		{
			DraftAction nextLevel = Match.CurrentDraft.PickedLevels[Match.RoundCounter()];
			// If valid, show the next level's name
			nextLevelMessage += new ChatMessage.Builder()
				.TextLine($"Starting Round {Match.RoundCounter() + 1}")
				.NewLine()
				.DashedLine().NewLine()
				.TextLine($"Level <{ShowdownColors.Cyan}>'{nextLevel.Level.Name}'</color>").NewLine()
				.TextLine($"picked by {nextLevel.Team.GetColoredTag()}")
				.Build().Message;
		}
		else if (winnerTeam.Wins > 1)
		{
			nextLevelMessage += "Upcoming -> Intermission";
		}
		else
		{
			Match.Initiative = Match.NonInitiative;
			nextLevelMessage +=
				new ChatMessage.Builder().TextLine("Upcoming -> Draftphase II").NewLine()
					.DashedLine().NewLine()
					.TextLine($"Initiative: {Match.Initiative.GetColoredTag()}").NewLine()
					.TextLine("Prepare yourself! It will start almost immediately!")
					.Build().Message;
		}


		// Create a cool and separated chat message using your original ChatMessage class
		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder()
				.ClearChat()
				.DashedLine().NewLine() // Dashed separator
				.TextLine($"<b>Round {currentRoundCounter}</b> over!").NewLine() // Centered round completion message
				.TextLine($"{winnerTeam.GetColoredTag()} scored").NewLine() // Display the winning team
				.DashedLine().NewLine() // Dashed line separating content
				.TextLine(nextLevelMessage).NewLine() // Show the next level or intermission
				.DashedLine().NewLine() // Dashed line to separate next section
				.TextLine($"{Match.Score()}") // Display the score
				.Build().Message); // Final dashed line for closure
	}

	public override void Exit()
	{
	}

	public override void OnRoundStarted()
	{
		InvokeFinish();
	}

	public override IState GetNextState()
	{
		if (Match.TeamA.Wins >= 2 || Match.TeamB.Wins >= 2)
		{
			return new StateMatchEnd(StateMachine);
		}

		if (Match.RoundCounter() < 2)
		{
			return new StateWarmUp(StateMachine);
		}

		return new StatePreDraft(StateMachine);
	}
}