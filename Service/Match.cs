using Showdown4.Entities;
using Showdown4.Statemachine;

namespace Showdown4.Service;

public class MatchService
{
    public MatchService(Team teamA, Team teamB)
    {
        TeamA = teamA;
        TeamB = teamB;
    }

    public Team TeamA { get; }
    public Team TeamB { get; }
}