using System;
using System.Collections.Generic;
using System.Linq;

namespace Showdown4.Entities;

public class Round
{
    public Round(int roundNumber)
    {
        RoundNumber = roundNumber;
        Leaderboard = new Dictionary<ulong, double>();
    }

    public int RoundNumber { get; set; }
    public Dictionary<ulong, double> Leaderboard { get; set; } // Dictionary<SteamId, BestTime>

    public void AddResult(Result result)
    {
        if (Leaderboard.ContainsKey(result.Racer.SteamId))
        {
            // Update the best time if the new time is better
            if (result.Time < Leaderboard[result.Racer.SteamId]) Leaderboard[result.Racer.SteamId] = result.Time;
        }
        else
        {
            // Add new racer and their time
            Leaderboard[result.Racer.SteamId] = result.Time;
        }
    }

    public int GetNumberOfFinishers()
    {
        return Leaderboard.Count;
    }

    public double GetRacerBestTime(ulong steamId)
    {
        return Leaderboard.ContainsKey(steamId) ? Leaderboard[steamId] : double.MaxValue;
    }

    // Method to get the best time for a racer in this round
    public double GetPersonalBest(Racer racer)
    {
        return GetRacerBestTime(racer.SteamId);
    }

    // Method to calculate the total time for a team in this round
    public double CalculateTotalTime(Team team)
    {
        double totalTime = 0.0;
        foreach (Racer racer in team.Racers)
        {
            double personalBest = GetPersonalBest(racer);
            if (personalBest < double.MaxValue) totalTime += personalBest;
        }

        return totalTime;
    }

    // Method to calculate the average time for a team in this round
    public double CalculateAverageTime(Team team)
    {
        double totalTime = CalculateTotalTime(team);
        return team.Racers.Count > 0 ? totalTime / team.Racers.Count : 0.0;
    }

    // Method to evaluate and sort teams based on performance in this round
    public List<Team> EvaluateTeamsSortedByWinner(Team teamA, Team teamB)
    {
        return CompareFinishers(teamA, teamB) ?? CompareTotalTeamTimes(teamA, teamB) ??
            CompareIndividualPlacements(teamA, teamB) ?? SelectRandomWinner(teamA, teamB);
    }

    // Compare the number of finishers between two teams
    private List<Team> CompareFinishers(Team teamA, Team teamB)
    {
        int finishersA = CountFinishers(teamA);
        int finishersB = CountFinishers(teamB);

        if (finishersA > finishersB) return new List<Team> { teamA, teamB };
        if (finishersB > finishersA) return new List<Team> { teamB, teamA };
        return null;
    }

    // Count the number of racers from a team who have finished
    private int CountFinishers(Team team)
    {
        return team.Racers.Count(racer => Leaderboard.ContainsKey(racer.SteamId));
    }

    // Compare the total team times between two teams
    private List<Team> CompareTotalTeamTimes(Team teamA, Team teamB)
    {
        double timeA = CalculateTotalTime(teamA);
        double timeB = CalculateTotalTime(teamB);

        if (timeA < timeB) return new List<Team> { teamA, teamB };
        if (timeB < timeA) return new List<Team> { teamB, teamA };
        return null;
    }

    // Compare individual placements (stubbed method)
    private List<Team> CompareIndividualPlacements(Team teamA, Team teamB)
    {
        // TODO: Implement Individual Placements comparison logic
        return SelectRandomWinner(teamA, teamB);
    }

    // Randomly select a winner between two teams
    private List<Team> SelectRandomWinner(Team teamA, Team teamB)
    {
        Random random = new();
        return random.Next(2) == 0 ? new List<Team> { teamA, teamB } : new List<Team> { teamB, teamA };
    }
}