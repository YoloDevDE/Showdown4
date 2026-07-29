using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Config;
using Showdown4.Managers;
using ZeepkistNetworking;

namespace Showdown4.Entities;

public class Match
{
	public Match(Team teamA, Team teamB)
	{
		TeamA = teamA;
		TeamB = teamB;
		Initiative = teamA;

		// The action inventory is a budget for the whole match, so it is handed out exactly once,
		// here. Everything a team does not spend in the first draft phase is still there when the
		// series ends in a tie and the tiebreaker has to be drafted - which is also what makes the
		// tiebreaker draft's shape a consequence of the format instead of a hardcoded special case.
		foreach (Team team in new[] { teamA, teamB })
		{
			team.Picks = MyConfig.DraftPicksPerTeamConfig.Value;
			team.Bans = MyConfig.DraftBansPerTeamConfig.Value;
			team.MissedDraft = false;
		}
	}

	public Team TeamA { get; set; }
	public Team TeamB { get; set; }
	public List<Round> Rounds { get; } = [];

	public List<Draft> Drafts { get; } = new();
	public Draft CurrentDraft => Drafts[^1];
	public Round CurrentRound => Rounds[^1];
	public List<OnlineZeeplevel> TrackedLevels { get; } = [];
	public Team Initiative { get; set; }
	public Team NonInitiative => Initiative == TeamA ? TeamB : TeamA;

	// Amount of rounds the match is played over and how many of them a team has to win to take it
	// (Bo3 -> 2, Bo5 -> 3, ...). Both come straight from the configured format.
	public int MaxRounds => MyConfig.MatchBestOfConfig.Value;
	public int RoundWinsNeeded => MaxRounds / 2 + 1;

	// 1-based number of the draft phase that is about to start, and of the one currently running.
	// They differ because Match.AddDraft() is what starts a phase: StatePreDraft runs before that,
	// StateDrafting after it.
	public int UpcomingDraftPhaseNumber => Drafts.Count + 1;
	public int CurrentDraftPhaseNumber => Drafts.Count;

	public string UpcomingDraftPhaseName => DraftPhaseName(UpcomingDraftPhaseNumber);
	public string CurrentDraftPhaseName => DraftPhaseName(CurrentDraftPhaseNumber);

	// The match is over as soon as one of the teams reached the needed amount of round wins.
	public bool HasWinner => IsWonBy(TeamA) || IsWonBy(TeamB);

	// The team that won the match, or null while it is still undecided. Read this instead of the
	// winner of the last round: it is also valid when no round has been played at all (e.g. the
	// showdown was stopped during the setup or the draft).
	public Team Winner => IsWonBy(TeamA) ? TeamA : IsWonBy(TeamB) ? TeamB : null;

	/// <summary>
	///     A new draft phase is due when the current draft holds no level for the upcoming round while
	///     the match is still undecided - i.e. the series ended in a tie (1:1 in a Bo3, 2:2 in a Bo5)
	///     and the decider has to be drafted. Derived from the drafted levels instead of a round
	///     number, so it holds for any format and any draft inventory.
	/// </summary>
	public bool NeedsNewDraftPhase => GetUpcomingLevelIndex() < 0;

	public bool IsWonBy(Team team)
	{
		return team.Wins >= RoundWinsNeeded;
	}

	// The first draft is the regular one. A later phase can only be reached with the series tied and
	// the drafted levels used up, so it is the one that produces the decider.
	private static string DraftPhaseName(int phaseNumber)
	{
		return phaseNumber <= 1 ? "Draftphase I" : "Tiebreaker Draft";
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

	/// <summary>
	///     Opens a new draft phase. The teams keep their inventory from the previous phase (it is a
	///     per-match budget, see the constructor), and they draft from the pool that is left: every
	///     level that was already picked or banned in an earlier phase stays off the table.
	/// </summary>
	public void AddDraft()
	{
		List<OnlineZeeplevel> allLevels =
			PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.CompetitionLevelsPlaylistNameConfig.Value);

		List<OnlineZeeplevel> unAvailableLevels = Drafts
			.SelectMany(draft => draft.PickedLevels.Concat(draft.BannedLevels))
			.Select(action => action.Level)
			.Where(level => level != null)
			.ToList();

		Draft draft = new(Initiative, NonInitiative, allLevels, unAvailableLevels)
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
	///     Name of the round that is about to start ("Round 1", "Round 2", ...). The last round of the
	///     format can only be reached with the series tied, so it is called the tiebreaker instead.
	/// </summary>
	public string UpcomingRoundName()
	{
		int roundNumber = Rounds.Count + 1;
		return roundNumber >= MaxRounds && MaxRounds > 1 ? "Tiebreaker" : $"Round {roundNumber}";
	}

	/// <summary>
	///     Index of the level the upcoming round is raced on, inside <see cref="CurrentDraft" />.
	///     Levels are picked per draft, so the index is the amount of rounds played since that draft
	///     began. The playlist locked in by <c>StateDraftCompleted</c> holds the picked levels in
	///     exactly that order, so this is the playlist index as well. Returns -1 when the current
	///     draft holds no level for that round (i.e. the match is over or a new draft is due).
	/// </summary>
	public int GetUpcomingLevelIndex()
	{
		if (Drafts.Count == 0)
		{
			return -1;
		}

		int index = Rounds.Count - CurrentDraft.PlayedRoundsBeforeDraft;
		return index >= 0 && index < CurrentDraft.PickedLevels.Count ? index : -1;
	}

	/// <summary>
	///     The level the upcoming round is raced on together with the team that picked it, or null when
	///     the current draft holds no level for that round.
	/// </summary>
	public DraftAction GetUpcomingLevel()
	{
		int index = GetUpcomingLevelIndex();
		return index < 0 ? null : CurrentDraft.PickedLevels[index];
	}

	/// <summary>
	///     The team that picked the level of the round that is about to start. Returns null when the
	///     level was not picked by one of the two teams (auto-/random pick).
	/// </summary>
	public Team GetPickerOfUpcomingLevel()
	{
		Team picker = GetUpcomingLevel()?.Team;
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