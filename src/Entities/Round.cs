using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Utils;

namespace Showdown4.Entities;

public class Round
{
    public readonly Team teamA;
    public readonly Team teamB;
    public List<Team> TeamsSortedByWinAsc;

    public Round(Team teamA, Team teamB)
    {
        Leaderboard = new Dictionary<ulong, Result>();
        this.teamA = teamA;
        this.teamB = teamB;
    }

    // Store the method used to determine the winner
    public WinningMethod WinningMethod { get; private set; }

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

    private List<Team> CompareFinishers()
    {
        int finishersA = GetFinishersCount(teamA);
        int finishersB = GetFinishersCount(teamB);

        if (finishersA > finishersB)
        {
            WinningMethod = WinningMethod.Finishers;
            return new List<Team> { teamA, teamB };
        }

        if (finishersB > finishersA)
        {
            WinningMethod = WinningMethod.Finishers;
            return new List<Team> { teamB, teamA };
        }

        return null;
    }

    private List<Team> CompareCumulativeTeamTimes()
    {
        double timeA = GetCumulativeTimeOfTeam(teamA);
        double timeB = GetCumulativeTimeOfTeam(teamB);

        if (timeA < timeB)
        {
            WinningMethod = WinningMethod.CumulativeTime;
            return new List<Team> { teamA, teamB };
        }

        if (timeB < timeA)
        {
            WinningMethod = WinningMethod.CumulativeTime;
            return new List<Team> { teamB, teamA };
        }

        return null;
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
                WinningMethod = WinningMethod.IndividualPlacements;
                return new List<Team> { teamA, teamB };
            }

            if (timeB < timeA)
            {
                WinningMethod = WinningMethod.IndividualPlacements;
                return new List<Team> { teamB, teamA };
            }
        }

        return null;
    }

    private List<Team> SelectRandomWinner()
    {
        WinningMethod = WinningMethod.RandomSelection;
        Random random = new Random();
        return random.Next(2) == 0 ? new List<Team> { teamA, teamB } : new List<Team> { teamB, teamA };
    }
}