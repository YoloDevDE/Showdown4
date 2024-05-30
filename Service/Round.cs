using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Showdown4.Entities;

namespace Showdown4.Service;

public class Round
{
    public Round(Team teamA, Team teamB, int roundNumber)
    {
        RacerRecords = new Dictionary<Racer, List<double>>
        {
            { teamA.RacerA, new List<double>() },
            { teamA.RacerB, new List<double>() },
            { teamB.RacerA, new List<double>() },
            { teamB.RacerB, new List<double>() }
        };
        RoundNumber = roundNumber;
    }

    public int RoundNumber { get; set; }

    public RoundEvaluator RoundEvaluator { get; set; }

    public Dictionary<Racer, List<double>> RacerRecords { get; }

    public void AddResult([NotNull] Racer racer, double result)
    {
        if (racer == null)
        {
            throw new ArgumentNullException(nameof(racer));
        }

        if (result > 0)
        {
            RacerRecords[racer].Add(result);
        }
    }

    public double GetPersonalBest([NotNull] Racer racer)
    {
        if (racer == null)
        {
            throw new ArgumentNullException(nameof(racer));
        }

        if (RacerRecords.ContainsKey(racer))
        {
            if (RacerRecords[racer].Count > 0)
            {
                return RacerRecords[racer].Min();
            }

            return 0;
        }

        throw new InvalidConstraintException();
    }
}