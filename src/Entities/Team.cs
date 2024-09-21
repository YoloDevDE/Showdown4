using System.Collections.Generic;

namespace Showdown4.Entities;

public class Team
{
    public Team(string name, string tag, string color)
    {
        Name = name;
        Tag = tag;
        Color = color;
        Racers = new List<Racer>();
    }

    public int Wins { get; set; }
    public int Losses { get; set; }
    public string Name { get; set; }
    public string Tag { get; set; }
    public string Color { get; set; }
    public uint Picks { get; set; } = 1;
    public uint Bans { get; set; } = 2;
    public List<Racer> Racers { get; set; }

    public int MaxTeamSize { get; set; } = 2;

    public string GetNameWithTag()
    {
        return $"[{Tag}] {Name}";
    }

    public string GetNameWithTagReverse()
    {
        return $"{Name} [{Tag}]";
    }

    public string GetNameWithNoTag()
    {
        return $"{Name}";
    }

    public void AddWin()
    {
        Wins += 1;
    }

    public void AddLoss()
    {
        Losses += 1;
    }

    public void AddRacer(Racer racer)
    {
        if (Racers.Count < MaxTeamSize) Racers.Add(racer);
    }

    public string GetLinkedRacersToString()
    {
        string result = "none";
        if (Racers.Count <= 0) return result;

        result = "";
        for (int index = 0; index < Racers.Count; index++)
        {
            Racer racer = Racers[index];
            result += racer.SteamName;
            if (index < Racers.Count - 1) result += ", ";
        }

        return result;
    }

    public string GetTag()
    {
        return $"[{Tag}]";
    }
}