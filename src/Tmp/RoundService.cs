using System;
using System.Collections.Generic;
using System.Linq;

namespace Showdown4.Tmp;

public class RoundService
{
    public double GetPersonalBestInRound(Racer racer, Round round)
    {
        return round.GetRacerBestTime(racer.SteamId);
    }

    public double CalculateTotalTime(Team team, Round round)
    {
        double totalTime = 0.0;

        foreach (Racer racer in team.Racers)
        {
            double personalBest = GetPersonalBestInRound(racer, round);
            if (personalBest < double.MaxValue)
            {
                totalTime += personalBest;
            }
        }

        return totalTime;
    }

    public double CalculateAverageTime(Team team, Round round)
    {
        double totalTime = CalculateTotalTime(team, round);
        return team.Racers.Count > 0 ? totalTime / team.Racers.Count : 0.0;
    }

    public List<Team> EvaluateTeamsSortedByWinner(Team teamA, Team teamB, Round round)
    {
        return CompareFinishers(teamA, teamB, round) ?? CompareTotalTeamTimes(teamA, teamB, round) ?? CompareIndividualPlacements(teamA, teamB, round) ?? SelectRandomWinner(teamA, teamB);
    }

    private List<Team> CompareFinishers(Team teamA, Team teamB, Round round)
    {
        int finishersA = CountFinishers(teamA, round);
        int finishersB = CountFinishers(teamB, round);

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

    private int CountFinishers(Team team, Round round)
    {
        return team.Racers.Count(racer => round.Leaderboard.ContainsKey(racer.SteamId));
    }

    private List<Team> CompareIndividualPlacements(Team teamA, Team teamB, Round round)
    {
        // TODO: Implement Individual Placements comparison logic
        return SelectRandomWinner(teamA, teamB);
    }

    private List<Team> CompareTotalTeamTimes(Team teamA, Team teamB, Round round)
    {
        double timeA = CalculateTotalTime(teamA, round);
        double timeB = CalculateTotalTime(teamB, round);

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

    private List<Team> SelectRandomWinner(Team teamA, Team teamB)
    {
        Random random = new Random();
        return random.Next(2) == 0 ? new List<Team> { teamA, teamB } : new List<Team> { teamB, teamA };
    }

    public void AddResult(Result result, Round round)
    {
        round.AddResult(result);
    }
}