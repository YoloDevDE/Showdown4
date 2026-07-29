using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Utils;

namespace Showdown4.Entities;

public class Round(Team teamA, Team teamB, Team levelPicker = null)
{
	// The team that picked the level raced in this round. Null when neither team picked it (the
	// auto-/random pick of the synthetic "Showdown" team).
	public readonly Team LevelPicker = levelPicker == teamA || levelPicker == teamB ? levelPicker : null;

	public readonly Team TeamA = teamA;
	public readonly Team TeamB = teamB;
	public List<Team> TeamsSortedByWinAsc;

	private Team _forcedWinner;

	public bool IsRoundOver { get; private set; } = false;

	// Best-of-X bookkeeping: a map is raced as several short sub-rounds, each one worth a point for
	// the team that won it. The map (= this round) goes to whoever won the majority of them. The
	// numbers live here instead of inside the racing state, because every sub-round runs in its own
	// freshly created state instance.
	public int SubRoundNumber { get; private set; } = 1;
	public int SubRoundWinsTeamA { get; private set; }
	public int SubRoundWinsTeamB { get; private set; }

	// Store the method used to determine the winner
	public WinningMethod WinningMethod { get; private set; }

	public Dictionary<ulong, Result> Leaderboard { get; set; } = new();

	public Team GetWinnerTeam => _forcedWinner ?? GetTeamsSortedByWinnerAsc().First();

	public void ForceWinner(Team team)
	{
		_forcedWinner = team;
	}

	// Awards the current sub-round. A null team means it ended without a winner at all.
	public void AwardSubRound(Team team)
	{
		if (team == TeamA)
		{
			SubRoundWinsTeamA++;
		}
		else if (team == TeamB)
		{
			SubRoundWinsTeamB++;
		}
	}

	public void BeginNextSubRound()
	{
		SubRoundNumber++;
		Leaderboard.Clear();
		TeamsSortedByWinAsc = null;
	}

	// True as soon as one team won the majority of the sub-rounds or all of them have been raced,
	// i.e. there is no point in racing another one.
	public bool IsSubRoundSeriesOver(int totalSubRounds)
	{
		int winsNeeded = totalSubRounds / 2 + 1;
		return SubRoundNumber >= totalSubRounds
		       || SubRoundWinsTeamA >= winsNeeded
		       || SubRoundWinsTeamB >= winsNeeded;
	}

	// The team that is currently ahead in the sub-round series, or null while it is tied.
	public Team GetSubRoundLeader()
	{
		if (SubRoundWinsTeamA == SubRoundWinsTeamB)
		{
			return null;
		}

		return SubRoundWinsTeamA > SubRoundWinsTeamB ? TeamA : TeamB;
	}

	public string GetSubRoundScore()
	{
		return $"{TeamA.GetColoredTag()} {SubRoundWinsTeamA} : {SubRoundWinsTeamB} {TeamB.GetColoredTag()}";
	}

	// Resolves a tie in which the leaderboard cannot decide anything (nobody finished at all): the
	// team that did <b>not</b> pick this level wins, because the picker is expected to know it best.
	// If neither team picked it, the better (= lower) average qualification time decides.
	public Team GetTieWinner()
	{
		if (LevelPicker == TeamA)
		{
			return TeamB;
		}

		if (LevelPicker == TeamB)
		{
			return TeamA;
		}

		double qualificationA = TeamA.QualificationTime;
		double qualificationB = TeamB.QualificationTime;

		if (qualificationA <= 0 && qualificationB <= 0)
		{
			return null;
		}

		if (qualificationA <= 0)
		{
			return TeamB;
		}

		if (qualificationB <= 0)
		{
			return TeamA;
		}

		return qualificationA < qualificationB ? TeamA : TeamB;
	}

	public void AddResult(Result result)
	{
		if (Leaderboard.TryAdd(result.Racer.SteamId, result))
		{
			return;
		}

		if (result.Time < Leaderboard[result.Racer.SteamId].Time)
		{
			Leaderboard[result.Racer.SteamId] = result;
		}
	}

	public double GetPersonalBest(Racer racer)
	{
		return Leaderboard.TryGetValue(racer.SteamId, out Result value) ? value.Time : double.MaxValue;
	}

	public double GetAvgTimeOfTeam(Team team)
	{
		int finishers = GetFinishersCount(team);
		return finishers > 0 ? GetCumulativeTimeOfTeam(team) / finishers : 0.0;
	}

	public int GetFinishersCount(Team team)
	{
		return team.Racers.Count(racer => Leaderboard.ContainsKey(racer.SteamId));
	}

	public void Evaluate(out List<Team> teams)
	{
		TeamsSortedByWinAsc = new List<Team>(CompareFinishers() ??
		                                     CompareCumulativeTeamTimes() ??
		                                     CompareIndividualPlacements() ??
		                                     SelectRandomWinner());
		teams = TeamsSortedByWinAsc;
	}

	public List<Team> GetTeamsSortedByWinnerAsc()
	{
		if (TeamsSortedByWinAsc == null)
		{
			Evaluate(out List<Team> teams);
		}

		return TeamsSortedByWinAsc;
	}

	private double GetCumulativeTimeOfTeam(Team team)
	{
		return team.Racers
			.Select(racer => GetPersonalBest(racer))
			.Where(personalBest => personalBest < double.MaxValue)
			.Sum();
	}

	private List<Team> CompareFinishers()
	{
		int finishersA = GetFinishersCount(TeamA);
		int finishersB = GetFinishersCount(TeamB);

		if (finishersA > finishersB)
		{
			WinningMethod = WinningMethod.Finishers;
			return new List<Team> { TeamA, TeamB };
		}

		if (finishersB <= finishersA)
		{
			return null;
		}

		WinningMethod = WinningMethod.Finishers;
		return new List<Team> { TeamB, TeamA };
	}

	private List<Team> CompareCumulativeTeamTimes()
	{
		double timeA = GetCumulativeTimeOfTeam(TeamA);
		double timeB = GetCumulativeTimeOfTeam(TeamB);

		if (timeA < timeB)
		{
			WinningMethod = WinningMethod.CumulativeTime;
			return new List<Team> { TeamA, TeamB };
		}

		if (timeB < timeA)
		{
			WinningMethod = WinningMethod.CumulativeTime;
			return new List<Team> { TeamB, TeamA };
		}

		return null;
	}

	private List<Team> CompareIndividualPlacements()
	{
		List<Racer> finishersA = TeamA.Racers
			.Where(racer => Leaderboard.ContainsKey(racer.SteamId))
			.OrderBy(racer => GetPersonalBest(racer))
			.ToList();

		List<Racer> finishersB = TeamB.Racers
			.Where(racer => Leaderboard.ContainsKey(racer.SteamId))
			.OrderBy(racer => GetPersonalBest(racer))
			.ToList();

		int minFinishers = Math.Min(finishersA.Count, finishersB.Count);

		for (int i = 0; i < minFinishers; i++)
		{
			double timeA = GetPersonalBest(finishersA[i]);
			double timeB = GetPersonalBest(finishersB[i]);

			if (timeA < timeB)
			{
				WinningMethod = WinningMethod.IndividualPlacements;
				return new List<Team> { TeamA, TeamB };
			}

			if (timeB < timeA)
			{
				WinningMethod = WinningMethod.IndividualPlacements;
				return new List<Team> { TeamB, TeamA };
			}
		}

		return null;
	}

	private List<Team> SelectRandomWinner()
	{
		WinningMethod = WinningMethod.RandomSelection;
		Random random = new();
		return random.Next(2) == 0 ? new List<Team> { TeamA, TeamB } : new List<Team> { TeamB, TeamA };
	}
}