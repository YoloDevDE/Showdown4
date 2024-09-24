using System.Collections.Generic;
using Showdown4.States.Showdown;

namespace Showdown4.Entities;

public class Match
{
    public Match(Team teamA, Team teamB)
    {
        TeamA = teamA;
        TeamB = teamB;
    }

    public Team TeamA { get; set; }
    public Team TeamB { get; set; }
    public List<Round> Rounds { get; } = [];

    public Draft FirstDraft { get; set; }
    public Draft SecondDraft { get; }

    public void AddRound(Round round)
    {
        Rounds.Add(round);
    }


    public int RoundCounter()
    {
        return Rounds.Count;
    }
}