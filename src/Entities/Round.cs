using System;
using System.Collections.Generic;
using System.Linq;

namespace Showdown4.Entities;

public class Round
{
    public Round()
    {
        Leaderboard = new Dictionary<ulong, Result>(); // Use Result to store racer and time data
    }


    public Dictionary<ulong, Result> Leaderboard { get; set; } // Dictionary<SteamId, Result>

    public void AddResult(Result result)
    {
        if (Leaderboard.TryAdd(result.Racer.SteamId, result))
        {
            return;
        }

        // Update the best time if the new time is better
        if (result.Time < Leaderboard[result.Racer.SteamId].Time)
        {
            Leaderboard[result.Racer.SteamId] = result;
        }
    }


    public double GetPersonalBest(Racer racer)
    {
        return Leaderboard.ContainsKey(racer.SteamId) ? Leaderboard[racer.SteamId].Time : double.MaxValue;
    }

    private double GetCumulativeTimeOfTeam(Team team)
    {
        return team.Racers
            .Select(racer => GetPersonalBest(racer))
            .Where(personalBest => personalBest < double.MaxValue)
            .Sum();
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

    public List<Team> GetTeamsSortedByWinnerAsc(Team teamA, Team teamB)
    {
        return CompareFinishers(teamA, teamB) ??
               CompareCumulativeTeamTimes(teamA, teamB) ??
               CompareIndividualPlacements(teamA, teamB) ??
               SelectRandomWinner(teamA, teamB);
    }

    private List<Team> CompareFinishers(Team teamA, Team teamB)
    {
        int finishersA = GetFinishersCount(teamA);
        int finishersB = GetFinishersCount(teamB);

        return finishersA > finishersB ? [teamA, teamB] : finishersB > finishersA ? [teamB, teamA] : null;
    }

    private List<Team> CompareCumulativeTeamTimes(Team teamA, Team teamB)
    {
        double timeA = GetCumulativeTimeOfTeam(teamA);
        double timeB = GetCumulativeTimeOfTeam(teamB);

        if (timeA < timeB)
        {
            return [teamA, teamB];
        }

        if (timeB < timeA)
        {
            return [teamB, teamA];
        }

        return null;
    }

    private List<Team> CompareIndividualPlacements(Team teamA, Team teamB)
    {
        // Get sorted finishers (racers with valid times)
        List<Racer> finishersA = teamA.Racers
            .Where(racer => Leaderboard.ContainsKey(racer.SteamId))
            .OrderBy(racer => GetPersonalBest(racer))
            .ToList();

        List<Racer> finishersB = teamB.Racers
            .Where(racer => Leaderboard.ContainsKey(racer.SteamId))
            .OrderBy(racer => GetPersonalBest(racer))
            .ToList();

        int minFinishers = Math.Min(finishersA.Count, finishersB.Count);

        // Compare racer positions in order (1st vs 1st, 2nd vs 2nd, etc.)
        for (int i = 0; i < minFinishers; i++)
        {
            double timeA = GetPersonalBest(finishersA[i]);
            double timeB = GetPersonalBest(finishersB[i]);

            // If one racer finishes faster, that team wins
            if (timeA < timeB)
            {
                return [teamA, teamB];
            }

            if (timeB < timeA)
            {
                return [teamB, teamA];
            }
        }


        return null;
    }

    private List<Team> SelectRandomWinner(Team teamA, Team teamB)
    {
        Random random = new Random();
        return random.Next(2) == 0 ? [teamA, teamB] : [teamB, teamA];
    }
}