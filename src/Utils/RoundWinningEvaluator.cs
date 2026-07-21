using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;

namespace Showdown4.Utils;

public static class RoundWinningCalculator
{
	public static List<Team> Evaluate(
		Team teamA,
		Team teamB,
		Dictionary<ulong, Result> leaderboard,
		out WinningMethod winningMethod)
	{
		// Try each tie‑breaker in order; first non‑null wins.
		List<Team> teams;

		teams = CompareFinishers(teamA, teamB, leaderboard, out winningMethod);
		if (teams != null) return teams;

		teams = CompareCumulativeTeamTimes(teamA, teamB, leaderboard, out winningMethod);
		if (teams != null) return teams;

		teams = CompareIndividualPlacements(teamA, teamB, leaderboard, out winningMethod);
		if (teams != null) return teams;

		// Fallback: random selection
		teams = SelectRandomWinner(teamA, teamB, out winningMethod);
		return teams;
	}

	private static List<Team> CompareFinishers(
		Team teamA,
		Team teamB,
		Dictionary<ulong, Result> leaderboard,
		out WinningMethod winningMethod)
	{
		int finishersA = GetFinishersCount(teamA, leaderboard);
		int finishersB = GetFinishersCount(teamB, leaderboard);

		if (finishersA > finishersB)
		{
			winningMethod = WinningMethod.Finishers;
			return new List<Team> { teamA, teamB };
		}

		if (finishersB > finishersA)
		{
			winningMethod = WinningMethod.Finishers;
			return new List<Team> { teamB, teamA };
		}

		winningMethod = default;
		return null;
	}

	private static List<Team> CompareCumulativeTeamTimes(
		Team teamA,
		Team teamB,
		Dictionary<ulong, Result> leaderboard,
		out WinningMethod winningMethod)
	{
		double timeA = GetCumulativeTimeOfTeam(teamA, leaderboard);
		double timeB = GetCumulativeTimeOfTeam(teamB, leaderboard);

		if (timeA < timeB)
		{
			winningMethod = WinningMethod.CumulativeTime;
			return new List<Team> { teamA, teamB };
		}

		if (timeB < timeA)
		{
			winningMethod = WinningMethod.CumulativeTime;
			return new List<Team> { teamB, teamA };
		}

		winningMethod = default;
		return null;
	}

	private static List<Team> CompareIndividualPlacements(
		Team teamA,
		Team teamB,
		Dictionary<ulong, Result> leaderboard,
		out WinningMethod winningMethod)
	{
		List<Racer> finishersA = teamA.Racers
			.Where(racer => leaderboard.ContainsKey(racer.SteamId))
			.OrderBy(racer => GetPersonalBest(racer, leaderboard))
			.ToList();

		List<Racer> finishersB = teamB.Racers
			.Where(racer => leaderboard.ContainsKey(racer.SteamId))
			.OrderBy(racer => GetPersonalBest(racer, leaderboard))
			.ToList();

		int minFinishers = Math.Min(finishersA.Count, finishersB.Count);

		for (int i = 0; i < minFinishers; i++)
		{
			double timeA = GetPersonalBest(finishersA[i], leaderboard);
			double timeB = GetPersonalBest(finishersB[i], leaderboard);

			if (timeA < timeB)
			{
				winningMethod = WinningMethod.IndividualPlacements;
				return new List<Team> { teamA, teamB };
			}

			if (timeB < timeA)
			{
				winningMethod = WinningMethod.IndividualPlacements;
				return new List<Team> { teamB, teamA };
			}
		}

		winningMethod = default;
		return null;
	}

	private static List<Team> SelectTieBreaker(Team teamA, Team teamB, out WinningMethod winningMethod)
	{
		winningMethod = default;
		return null;
	}


	private static List<Team> SelectRandomWinner(
		Team teamA,
		Team teamB,
		out WinningMethod winningMethod)
	{
		winningMethod = WinningMethod.RandomSelection;

		// NOTE: if you want deterministic behaviour per round,
		// inject a Random instance instead of creating it here.
		Random random = new();
		return random.Next(2) == 0
			? new List<Team> { teamA, teamB }
			: new List<Team> { teamB, teamA };
	}

	private static int GetFinishersCount(
		Team team,
		Dictionary<ulong, Result> leaderboard)
	{
		return team.Racers.Count(racer => leaderboard.ContainsKey(racer.SteamId));
	}

	private static double GetCumulativeTimeOfTeam(
		Team team,
		Dictionary<ulong, Result> leaderboard)
	{
		return team.Racers
			.Select(racer => GetPersonalBest(racer, leaderboard))
			.Where(personalBest => personalBest < double.MaxValue)
			.Sum();
	}

	private static double GetPersonalBest(
		Racer racer,
		Dictionary<ulong, Result> leaderboard)
	{
		return leaderboard.TryGetValue(racer.SteamId, out Result result)
			? result.Time
			: double.MaxValue;
	}
}