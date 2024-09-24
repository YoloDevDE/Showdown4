using System;
using System.Collections.Generic;
using System.Linq;

namespace Showdown4.Entities;

public class Round
{
    public Round(int roundNumber)
    {
        RoundNumber = roundNumber;
        Leaderboard = new Dictionary<ulong, Result>(); // Use Result to store racer and time data
    }

    public int RoundNumber { get; set; }
    public Dictionary<ulong, Result> Leaderboard { get; set; } // Dictionary<SteamId, Result>

    public void AddResult(Result result)
    {
        if (Leaderboard.ContainsKey(result.Racer.SteamId))
        {
            // Update the best time if the new time is better
            if (result.Time < Leaderboard[result.Racer.SteamId].Time)
            {
                Leaderboard[result.Racer.SteamId] = result;
            }
        }
        else
        {
            // Add new racer and their time
            Leaderboard[result.Racer.SteamId] = result;
        }
    }

    public int GetNumberOfFinishers()
    {
        return Leaderboard.Count;
    }

    public double GetRacerBestTime(ulong steamId)
    {
        return Leaderboard.ContainsKey(steamId) ? Leaderboard[steamId].Time : double.MaxValue;
    }

    // Method to get the best time for a racer in this round
    public double GetPersonalBest(Racer racer)
    {
        return GetRacerBestTime(racer.SteamId);
    }

    // Calculate total and average time for teams, etc.
    public double CalculateTotalTime(Team team)
    {
        double totalTime = 0.0;
        foreach (Racer racer in team.Racers)
        {
            double personalBest = GetPersonalBest(racer);
            if (personalBest < double.MaxValue)
            {
                totalTime += personalBest;
            }
        }

        return totalTime;
    }

    public double CalculateAverageTime(Team team)
    {
        int finishers = CountFinishers(team);
        return finishers > 0 ? CalculateTotalTime(team) / finishers : 0.0;
    }

    public int CountFinishers(Team team)
    {
        return team.Racers.Count(racer => Leaderboard.ContainsKey(racer.SteamId));
    }

    public List<Team> EvaluateTeamsSortedByWinner(Team teamA, Team teamB)
    {
        return CompareFinishers(teamA, teamB) ??
               CompareTotalTeamTimes(teamA, teamB) ??
               CompareIndividualPlacements(teamA, teamB) ?? SelectRandomWinner(teamA, teamB);
    }

    private List<Team> CompareFinishers(Team teamA, Team teamB)
    {
        int finishersA = CountFinishers(teamA);
        int finishersB = CountFinishers(teamB);

        if (finishersA > finishersB)
        {
            return new List<Team> { teamA, teamB };
        }

        if (finishersB > finishersA)
        {
            return new List<Team> { teamB, teamA };
        }

        return null;
    }

    private List<Team> CompareTotalTeamTimes(Team teamA, Team teamB)
    {
        double timeA = CalculateTotalTime(teamA);
        double timeB = CalculateTotalTime(teamB);

        if (timeA < timeB)
        {
            return new List<Team> { teamA, teamB };
        }

        if (timeB < timeA)
        {
            return new List<Team> { teamB, teamA };
        }

        return null;
    }

    private List<Team> CompareIndividualPlacements(Team teamA, Team teamB)
    {
        // This method is a placeholder for further refinement
        return SelectRandomWinner(teamA, teamB);
    }

    private List<Team> SelectRandomWinner(Team teamA, Team teamB)
    {
        Random random = new Random();
        return random.Next(2) == 0 ? new List<Team> { teamA, teamB } : new List<Team> { teamB, teamA };
    }
}