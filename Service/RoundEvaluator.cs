using System;
using System.Collections.Generic;
using Showdown4.Entities;
using ZeepSDK.Chat;

namespace Showdown4.Service;

public class RaceEvaluator
{
    private readonly Team TeamA;
    private readonly Team TeamB;

    public RaceEvaluator(Team teamA, Team teamB)
    {
        TeamA = teamA;
        TeamB = teamB;
    }

    public Team CurrentWinner { get; private set; }
    public Team CurrentLoser { get; private set; }

    public List<Team> EvaluateCurrentTeams(Round round)
    {
        List<Team> currentTeamsOrderedByWinner = CompareFinishers() ?? CompareTotalTime() ?? CompareIndividualPlacements(round) ?? SelectRandomWinner();
        CurrentWinner = currentTeamsOrderedByWinner[0];
        CurrentLoser = currentTeamsOrderedByWinner[1];
        return currentTeamsOrderedByWinner;
    }

    private List<Team> CompareFinishers()
    {
        int finishersA = TeamA.GetFinisherCount();
        int finishersB = TeamB.GetFinisherCount();

        ChatApi.SendMessage("CompareFinishers");

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
        ChatApi.SendMessage("CompareTotalTime");
        double timeA = TeamA.GetTotalTime();
        double timeB = TeamB.GetTotalTime();

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

    private List<Team> CompareIndividualPlacements(Round round)
    {
        ChatApi.SendMessage("CompareIndividualPlacements");
        double bestResultA = Math.Min(round.GetPersonalBest(TeamA.RacerA), round.GetPersonalBest(TeamA.RacerB));
        double bestResultB = Math.Min(round.GetPersonalBest(TeamB.RacerA), round.GetPersonalBest(TeamB.RacerB));

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
        ChatApi.SendMessage("SelectRandomWinner");
        Random random = new Random();
        if (random.Next(2) == 0)
        {
            return new List<Team> { TeamA, TeamB };
        }

        return new List<Team> { TeamB, TeamA };
    }
}