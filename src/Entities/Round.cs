using System;
using System.Collections.Generic;
using System.Linq;

namespace Showdown4.Entities;

public class Round
{
    private readonly Team teamA;
    private readonly Team teamB;

    public Round(Team teamA, Team teamB)
    {
        Leaderboard = new Dictionary<ulong, Result>();
        this.teamA = teamA;
        this.teamB = teamB;
    }

    public Dictionary<ulong, Result> Leaderboard { get; set; }

    public Team GetWinnerTeam => GetTeamsSortedByWinnerAsc().First();

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

    public List<Team> GetTeamsSortedByWinnerAsc()
    {
        return CompareFinishers() ??
               CompareCumulativeTeamTimes() ??
               CompareIndividualPlacements() ??
               SelectRandomWinner();
    }

    private List<Team> CompareFinishers()
    {
        int finishersA = GetFinishersCount(teamA);
        int finishersB = GetFinishersCount(teamB);

        return finishersA > finishersB ? new List<Team> { teamA, teamB } :
            finishersB > finishersA ? new List<Team> { teamB, teamA } :
            null;
    }

    private List<Team> CompareCumulativeTeamTimes()
    {
        double timeA = GetCumulativeTimeOfTeam(teamA);
        double timeB = GetCumulativeTimeOfTeam(teamB);

        return timeA < timeB ? new List<Team> { teamA, teamB } :
            timeB < timeA ? new List<Team> { teamB, teamA } :
            null;
    }

    private List<Team> CompareIndividualPlacements()
    {
        List<Racer> finishersA = teamA.Racers
            .Where(racer => Leaderboard.ContainsKey(racer.SteamId))
            .OrderBy(racer => GetPersonalBest(racer))
            .ToList();

        List<Racer> finishersB = teamB.Racers
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
                return new List<Team> { teamA, teamB };
            }

            if (timeB < timeA)
            {
                return new List<Team> { teamB, teamA };
            }
        }

        return null;
    }

    private List<Team> SelectRandomWinner()
    {
        Random random = new Random();
        return random.Next(2) == 0 ? new List<Team> { teamA, teamB } : new List<Team> { teamB, teamA };
    }
}