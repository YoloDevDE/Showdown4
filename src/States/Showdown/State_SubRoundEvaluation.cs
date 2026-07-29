using System.Collections.Generic;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

/// <summary>
///     The short phase right after the time of a racing sub-round ran out. The result is saved and
///     evaluated immediately when this state is entered - while the leaderboard of the sub-round is
///     still untouched - and only afterwards everyone is wiped from the leaderboard and respawned for
///     the next sub-round. Once every sub-round of the map has been raced, the map point is awarded
///     and we continue with the podium phase instead, which then skips to the next map.
/// </summary>
public class StateSubRoundEvaluation(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private bool _isMapOver;
	private Round _round;
	private int _subRoundNumber;
	private Team _subRoundWinner;

	private static int SubRoundCount => MyConfig.RacingSubRoundsConfig.Value;

	public override void Enter()
	{
		_round = Match.CurrentRound;
		_subRoundNumber = _round.SubRoundNumber;

		// The sub-round is over, so nobody may set a time anymore. The racers stay blocked until
		// they have been respawned for the next sub-round.
		RacerResetService.BlockEveryoneFromSettingTime();

		// The result is evaluated right here, at the very start of this state.
		_subRoundWinner = EvaluateSubRoundWinner();
		_round.AwardSubRound(_subRoundWinner);
		_isMapOver = _round.IsSubRoundSeriesOver(SubRoundCount);

		AnnounceSubRoundResult();

		if (_isMapOver)
		{
			// The map is decided: whoever won the most sub-rounds gets the round point. Only if even
			// the sub-round series ended in a tie the map-pick/qualification rule decides.
			_round.ForceWinner(_round.GetSubRoundLeader() ?? _round.GetTieWinner());
		}
		else
		{
			ResetForNextSubRound();
		}

		Countdown.Start(MyConfig.SubRoundEvaluationSecondsConfig.Value, UpdateCountdownMessage, InvokeFinish);
	}

	public override IState GetNextState()
	{
		return _isMapOver ? new StatePostRacing(StateMachine) : new StateRacing(StateMachine, false);
	}

	public override void OnRoundEnded()
	{
		// Safety net: the host skipped the level while we were saving the result. Nothing to race
		// anymore on this map, so close it with the score we have.
		if (!_isMapOver)
		{
			_isMapOver = true;
			_round.ForceWinner(_round.GetSubRoundLeader() ?? _round.GetTieWinner());
		}

		Countdown.Stop();
		InvokeFinish();
	}

	// Determines who won the sub-round. Every situation the leaderboard cannot decide (nobody
	// finished at all, or both teams are completely equal) falls back to the map-pick rule instead
	// of the random pick of the regular round evaluation.
	private Team EvaluateSubRoundWinner()
	{
		if (_round.GetFinishersCount(_round.TeamA) + _round.GetFinishersCount(_round.TeamB) == 0)
		{
			return _round.GetTieWinner();
		}

		_round.Evaluate(out List<Team> sortedTeams);
		return _round.WinningMethod == WinningMethod.RandomSelection
			? _round.GetTieWinner()
			: sortedTeams[0];
	}

	// Everything the next sub-round needs: an empty leaderboard, respawned racers and the
	// permission to set times again.
	private void ResetForNextSubRound()
	{
		RacerResetService.ClearLeaderboardForEveryone();
		RacerResetService.RespawnEveryone();
		RacerResetService.UnblockEveryoneFromSettingTime();

		_round.BeginNextSubRound();
	}

	private void AnnounceSubRoundResult()
	{
		string resultLine = _subRoundWinner == null
			? "Nobody won this race"
			: $"{_subRoundWinner.GetColoredTag()} won this race";

		string upcomingLine = _isMapOver
			? "Closing the map..."
			: $"Race {_round.SubRoundNumber + 1}/{SubRoundCount} starts in a moment";

		ChatMessage.SendCustomMessage(new ChatMessage.Builder()
			.ClearChat()
			.DashedLine().NewLine()
			.TextLine($"<b>Race {_subRoundNumber}/{SubRoundCount}</b> over!").NewLine()
			.TextLine(resultLine).NewLine()
			.TextLine(_round.GetSubRoundScore()).NewLine()
			.DashedLine().NewLine()
			.TextLine(upcomingLine)
			.Build().Message);
	}

	private void UpdateCountdownMessage(int secondsRemaining)
	{
		if (secondsRemaining < 0)
		{
			return;
		}

		new ServerMessage("center")
			.ShowdownHeader(false, "center")
			.AddSeparator()
			.AddLine(line => line
				.AddBlock($"Race {_subRoundNumber}/{SubRoundCount} finished",
					b => b.Bold().Color(ShowdownColors.Cyan)))
			.AddLine(line => line
				.AddBlock(_subRoundWinner == null
					? "Saving result - nobody won this race"
					: $"Saving result - {_subRoundWinner.GetColoredTag()} scored", b => b.Color(ShowdownColors.Gray)))
			.AddSeparator()
			.AddLine(line => line.AddBlock(_round.GetSubRoundScore()))
			.Send();
	}
}