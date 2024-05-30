using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Showdown4.Entities;
using ZeepSDK.External.FluentResults;

namespace Showdown4.Service;

public class Race
{
    public Dictionary<Racer, List<double>> RacerResults { get; private set; } = new Dictionary<Racer, List<double>>();

    public Race()
    {
    }

    public void AddResult([NotNull] Racer racer, double result)
    {
        if (racer == null)
        {
            throw new ArgumentNullException(nameof(racer));
        }

        if (result > 0)
        {
            RacerResults[racer].Add(result);
        }
    }

    public double GetPersonalBest([NotNull] Racer racer)
    {
        if (racer == null)
        {
            throw new ArgumentNullException(nameof(racer));
        }

        if (RacerResults.ContainsKey(racer))
            return RacerResults[racer].Min();
        throw new InvalidConstraintException();
    }
}