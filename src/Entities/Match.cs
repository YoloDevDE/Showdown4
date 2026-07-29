using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Config;
using Showdown4.Managers;
using ZeepkistNetworking;

namespace Showdown4.Entities;

public class Match(Team teamA, Team teamB)
{
	// A match is a best-of-three: two round wins take it, so the third round is always the decider.
	public const int WinsNeededToWinMatch = 2;
	public const int DecidingRoundNumber = 3;

	public Team TeamA { get; set; } = teamA;
	public Team TeamB { get; set; } = teamB;
	public List<Round> Rounds { get; } = [];

	public List<Draft> Drafts { get; } = new();
	public Draft CurrentDraft => Drafts[^1];
	public Round CurrentRound => Rounds[^1];
	public List<OnlineZeeplevel> TrackedLevels { get; } = [];
	public Team Initiative { get; set; } = teamA;
	public Team NonInitiative => Initiative == TeamA ? TeamB : TeamA;

	// Draftphase I is the very first draft; every draft after a round has been played is Draftphase II.
	public bool IsDraftphaseTwo => Rounds.Count > 0;
	public string DraftphaseName => IsDraftphaseTwo ? "Draftphase II" : "Draftphase I";

	// The match is over as soon as one of the teams reached the needed amount of round wins.
	public bool HasWinner => IsWonBy(TeamA) || IsWonBy(TeamB);

	// Draftphase I covers every round before the decider, so only the decider needs a new draft.
	public bool NeedsNewDraftPhase => Rounds.Count >= DecidingRoundNumber - 1;

	public bool IsWonBy(Team team)
	{
		return team.Wins >= WinsNeededToWinMatch;
	}

	public string ScoreWithFullNameColored()
	{
		return $"{TeamA.GetFullColoredTagAndName()} {TeamA.Wins}:{TeamB.Wins} {TeamB.GetFullColoredTagAndName()}";
	}

	public string ScoreWithFullNameColoredAndPadded()
	{
		int maxLength = Math.Max(TeamA.GetFullColoredTagAndName().Length, TeamB.GetFullColoredTagAndName().Length);
		return
			$"{TeamA.GetFullColoredTagAndName().PadLeft(maxLength)} {TeamA.Wins}:{TeamB.Wins} {TeamB.GetFullColoredTagAndName().PadRight(maxLength)}";
	}

	public string ScoreColored()
	{
		return
			$"<{TeamA.Color}>{TeamA.GetTag()}</color> {TeamA.Wins}:{TeamB.Wins} <{TeamB.Color}>{TeamB.GetTag()}</color>";
	}

	public string Score()
	{
		return $"{TeamA.GetColoredTag()} {TeamA.Wins}:{TeamB.Wins} {TeamB.GetColoredTag()}";
	}

	public string ScoreOnly()
	{
		return $"{TeamA.Wins}:{TeamB.Wins}";
	}

	public void AddRound(Round round)
	{
		Rounds.Add(round);
	}

	public void AddDraft()
	{
		List<OnlineZeeplevel> unAvaiableLevels = new();
		List<OnlineZeeplevel> allLevels =
			PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.CompetitionLevelsPlaylistNameConfig.Value);
		if (Drafts.Count > 0)
		{
			unAvaiableLevels =
				new List<OnlineZeeplevel>(CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));
		}

		Draft draft = new(Initiative, NonInitiative, allLevels, unAvaiableLevels)
		{
			PlayedRoundsBeforeDraft = Rounds.Count
		};
		Drafts.Add(draft);
	}

	public int RoundCounter()
	{
		return Rounds.Count;
	}

	/// <summary>
	///     Name of the round that is about to start ("Round 1", "Round 2", ...). A best-of-three is
	///     decided in the third round at the latest, so that one is called the tiebreaker instead.
	/// </summary>
	public string UpcomingRoundName()
	{
		int roundNumber = Rounds.Count + 1;
		return roundNumber == DecidingRoundNumber ? "Tiebreaker" : $"Round {roundNumber}";
	}

	/// <summary>
	///     The team that picked the level of the round that is about to start. Levels are picked per
	///     draft, so the index inside the current draft is the amount of rounds played since it began.
	///     Returns null when the level was not picked by one of the two teams (auto-/random pick).
	/// </summary>
	public Team GetPickerOfUpcomingLevel()
	{
		if (Drafts.Count == 0)
		{
			return null;
		}

		int index = Rounds.Count - CurrentDraft.PlayedRoundsBeforeDraft;
		if (index < 0 || index >= CurrentDraft.PickedLevels.Count)
		{
			return null;
		}

		Team picker = CurrentDraft.PickedLevels[index].Team;
		return picker == TeamA || picker == TeamB ? picker : null;
	}

	public Team GetTeamBySteamId(ulong steamId)
	{
		if (TeamA.Racers.Any(player => player.SteamId == steamId))
		{
			return TeamA;
		}

		if (TeamB.Racers.Any(player => player.SteamId == steamId))
		{
			return TeamB;
		}

		return null;
	}
}