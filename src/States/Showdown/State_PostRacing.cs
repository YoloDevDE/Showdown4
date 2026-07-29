using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

/// <summary>
///     The podium phase after the last sub-round of a map. The game gets a moment to settle before
///     the final leaderboard is read, the round point is awarded and the result is announced.
/// </summary>
public class StatePostRacing(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// Guards the winner evaluation so it happens exactly once, even if the next round starts
	// before the podium countdown has finished (in which case we still announce first).
	private bool _resultAnnounced;

	public override void Enter()
	{
		// The overtake overrides on the in-game leaderboard have already been stopped/reverted by
		// StateRacing.Exit(). We now let the game show its podium for a moment before we fetch the
		// final leaderboard and announce the winner, so nothing is evaluated while the podium phase
		// is still settling. Until then we only tell everyone that the round is being closed.
		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder()
				.ClearChat()
				.DashedLine().NewLine()
				.TextLine("<b>Closing Round</b>").NewLine()
				.TextLine($"<{ShowdownColors.Cyan}>Fetching Leaderboard...</color>").NewLine()
				.TextLine("Determining winner...").NewLine()
				.DashedLine()
				.Build().Message);

		Countdown.Start(MyConfig.PostRacingPodiumDurationConfig.Value, onComplete: AnnounceRoundResult);
	}

	public override void OnRoundStarted()
	{
		// Safety net: if the game moves on before the podium countdown elapsed, make sure the
		// winner is still evaluated and announced before we leave this state.
		AnnounceRoundResult();
		InvokeFinish();
	}

	public override IState GetNextState()
	{
		if (Match.HasWinner)
		{
			return new StateMatchEnd(StateMachine);
		}

		return Match.NeedsNewDraftPhase
			? new StatePreDraft(StateMachine)
			: new StatePreRacing(StateMachine);
	}

	// Runs after the podium phase has elapsed: only now do we read the final leaderboard, award the
	// win and announce who won, so the result is never determined while overrides are still active.
	private void AnnounceRoundResult()
	{
		if (_resultAnnounced)
		{
			return;
		}

		_resultAnnounced = true;
		Countdown.Stop();

		Team winnerTeam = Match.CurrentRound.GetWinnerTeam;
		winnerTeam.AddWin();

		int currentRoundCounter = Match.RoundCounter();

		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder()
				.ClearChat()
				.DashedLine().NewLine()
				.TextLine($"<b>Round {currentRoundCounter}</b> over!").NewLine()
				.TextLine($"{winnerTeam.GetColoredTag()} scored").NewLine()
				.DashedLine().NewLine()
				.TextLine(BuildUpcomingMessage(winnerTeam)).NewLine()
				.DashedLine().NewLine()
				.TextLine($"{Match.Score()}")
				.Build().Message);
	}

	// Describes what happens next: the level of the round that is already drafted, the intermission
	// after a decided match, or the second draft phase (which hands the initiative over).
	private string BuildUpcomingMessage(Team winnerTeam)
	{
		if (Match.RoundCounter() < Match.CurrentDraft.PickedLevels.Count)
		{
			DraftAction nextLevel = Match.CurrentDraft.PickedLevels[Match.RoundCounter()];
			return new ChatMessage.Builder()
				.TextLine($"Starting {Match.UpcomingRoundName()}")
				.NewLine()
				.DashedLine().NewLine()
				.TextLine($"Level <{ShowdownColors.Cyan}>'{nextLevel.Level.Name}'</color>").NewLine()
				.TextLine($"picked by {nextLevel.Team.GetColoredTag()}")
				.Build().Message;
		}

		if (Match.IsWonBy(winnerTeam))
		{
			return "Upcoming -> Intermission";
		}

		Match.Initiative = Match.NonInitiative;
		return new ChatMessage.Builder()
			.TextLine("Upcoming -> Draftphase II").NewLine()
			.DashedLine().NewLine()
			.TextLine($"Initiative: {Match.Initiative.GetColoredTag()}").NewLine()
			.TextLine("Prepare yourself! It will start almost immediately!")
			.Build().Message;
	}
}