using System;
using System.Collections.Generic;
using Showdown4.Entities;
using ZeepkistClient;

namespace Showdown4.Service;

public class RoundEvaluator
{
    private readonly Round Round;
    private readonly Team TeamA;
    private readonly Team TeamB;

    public RoundEvaluator(Team teamA, Team teamB, Round round)
    {
        TeamA = teamA;
        TeamB = teamB;
        Round = round;
    }

    public Team CurrentWinner { get; private set; }
    public Team CurrentLoser { get; private set; }

    public List<Team> EvaluateCurrentTeams()
    {
        List<Team> currentTeamsOrderedByWinner = CompareFinishers() ?? CompareTotalTime() ?? CompareIndividualPlacements() ?? SelectRandomWinner();
        CurrentWinner = currentTeamsOrderedByWinner[0];
        CurrentLoser = currentTeamsOrderedByWinner[1];
        return currentTeamsOrderedByWinner;
    }

    private List<Team> CompareFinishers()
    {
        int finishersA = GetFinisherCount(TeamA);
        int finishersB = GetFinisherCount(TeamB);


        if (finishersA > finishersB)
        {
            return new List<Team> { TeamA, TeamB };
        }

        if (finishersB > finishersA)
        {
            return new List<Team> { TeamB, TeamA };
        }

        return null;
    }

    private List<Team> CompareTotalTime()
    {
        double timeA = GetTotalTime(TeamA);
        double timeB = GetTotalTime(TeamB);

        if (timeA < timeB)
        {
            return new List<Team> { TeamA, TeamB };
        }

        if (timeB < timeA)
        {
            return new List<Team> { TeamB, TeamA };
        }

        return null;
    }

    private List<Team> CompareIndividualPlacements()
    {
        double bestResultA = Math.Min(Round.GetPersonalBest(TeamA.RacerA), Round.GetPersonalBest(TeamA.RacerB));
        double bestResultB = Math.Min(Round.GetPersonalBest(TeamB.RacerA), Round.GetPersonalBest(TeamB.RacerB));

        if (bestResultA < bestResultB)
        {
            return new List<Team> { TeamA, TeamB };
        }

        if (bestResultB < bestResultA)
        {
            return new List<Team> { TeamB, TeamA };
        }

        return null;
    }

    private List<Team> SelectRandomWinner()
    {
        Random random = new Random();
        if (random.Next(2) == 0)
        {
            return new List<Team> { TeamA, TeamB };
        }

        return new List<Team> { TeamB, TeamA };
    }

    public double GetAverageTime(Team team)
    {
        return (Round.GetPersonalBest(team.RacerA) + Round.GetPersonalBest(team.RacerB)) * (HasValidResults(team) ? 0.5 : 1);
    }

    public int GetFinisherCount(Team team)
    {
        int count = 0;
        if (Round.RacerRecords[team.RacerA].Count > 0)
        {
            count++;
        }

        if (Round.RacerRecords[team.RacerB].Count > 0)
        {
            count++;
        }

        return count;
    }

    public void UpdateResults(ZeepkistNetworkPlayer networkPlayer)
    {
        if (networkPlayer.CurrentResult == null || networkPlayer.CurrentResult.Time == 0)
        {
            return;
        }

        List<Racer> racers = new List<Racer> { TeamA.RacerA, TeamA.RacerB, TeamB.RacerA, TeamB.RacerB };
        foreach (Racer racer in racers)
        {
            if (racer.SteamId == networkPlayer.SteamID && (networkPlayer.CurrentResult.Time <= Round.GetPersonalBest(racer) || Round.GetPersonalBest(racer) == 0))
            {
                Round.AddResult(racer, networkPlayer.CurrentResult.Time);
            }
        }
    }

    public double GetTotalTime(Team team)
    {
        double totalTime = 0;
        if (Round.GetPersonalBest(team.RacerA) > 0)
        {
            totalTime += Round.GetPersonalBest(team.RacerA);
        }

        if (Round.GetPersonalBest(team.RacerB) > 0)
        {
            totalTime += Round.GetPersonalBest(team.RacerB);
        }

        return totalTime;
    }

    public bool HasValidResults(Team team)
    {
        return Round.RacerRecords[team.RacerA].Count > 0 && Round.RacerRecords[team.RacerB].Count > 0;
    }
}