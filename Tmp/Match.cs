using System.Collections.Generic;

namespace Showdown4.Tmp;

public class Match
{
    public Match(Team teamA, Team teamB, int bestOf)
    {
        TeamA = teamA;
        TeamB = teamB;
        BestOf = bestOf;
        Rounds = new List<Round>();
    }

    public Team TeamA { get; set; }
    public Team TeamB { get; set; }
    public int BestOf { get; set; }
    public List<Round> Rounds { get; set; }

    public Round GetCurrentRound()
    {
        return Rounds[^1];
    }

    public int RoundCounter()
    {
        if (Rounds == null || Rounds.Count < 1)
        {
            return 1;
        }

        return Rounds.Count;
    }
}