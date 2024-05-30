namespace Showdown4.Entities;

public class Team
{
    public Team(Racer racerA, Racer racerB)
    {
        RacerA = racerA;
        RacerB = racerB;
    }

    public Racer RacerA { get; set; }
    public Racer RacerB { get; set; }
    public string Name { get; set; }
    public string Tag { get; set; }
    public string Color { get; set; }

    public int Wins { get; set; }
    public int Losses { get; set; }

    public bool TeamCompleted { get; private set; }

    public int GetLinkedRacersCount()
    {
        int result = 0;
        if (RacerA != null)
        {
            result++;
        }

        if (RacerB != null)
        {
            result++;
        }

        return result;
    }

    public string GetLinkedRacersToString()
    {
        string result = "none";
        if (RacerA != null)
        {
            result = RacerA.SteamName;
        }

        if (RacerB != null)
        {
            result = result + ", " + RacerB.SteamName;
        }

        return result;
    }

    public void AddWin()
    {
        Wins += 1;
    }

    public void AddLoss()
    {
        Losses += 1;
    }


    public string GetNameWithTag()
    {
        return $"[{Tag}] {Name}";
    }

    public string GetNameWithNoTag()
    {
        return $"{Name}";
    }

    public string GetTag()
    {
        return $"[{Tag}]";
    }

    public override string ToString()
    {
        return $"<br>{nameof(Name)}: {Name}<br>{nameof(Tag)}: {Tag}<br>{nameof(Color)}: {Color}";
    }

    public string AddRacer(Racer racer)
    {
        // if ((RacerA != null && RacerA.Equals(racer)) || (RacerB != null && RacerB.Equals(racer)))
        //     return $"'{racer.SteamName}' is already linked to '{GetNameWithTag()}'";

        if (RacerA == null)
        {
            RacerA = racer;
        }
        else if (RacerB == null)
        {
            RacerB = racer;
            TeamCompleted = true;
        }
        else
        {
            return "Team is already completed!";
        }

        return $"{racer.SteamName} linked to {GetNameWithTag()}";
    }

    public string AddRacer(string steamName, ulong steamId)
    {
        return AddRacer(new Racer(steamName, steamId));
    }
}