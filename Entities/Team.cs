namespace Showdown4.Statemachine;

public class Team
{
    public Team(Racer racerA, Racer racerB)
    {
        RacerA = racerA;
        RacerB = racerB;
    }

    public double GetResult()
    {
        return (RacerA.Result + RacerB.Result) * 0.5;
    }

    public Racer RacerA { get; }
    public Racer RacerB { get; }
}