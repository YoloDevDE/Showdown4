using System.Collections.Generic;
using System.Linq;

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
    public int Picks { get; set; } = 1;
    public int Bans { get; set; } = 2;
    public bool MissedDraft { get; set; } = false;
    public List<Racer> Racers { get; set; }

    public int MaxTeamSize { get; set; } = 2;

    public string GetNameWithTag()
    {
        string shortName = Name.Length > 32 ? $"{Name[..32]}..." : Name;
        return $"{GetTag()} {shortName}";
    }

    public string GetFullNameWithTag()
    {
        return $"{GetTag()} {Name}";
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
        if (Racers.Count < MaxTeamSize)
        {
            Racers.Add(racer);
        }
    }

    public void RemoveRacer(ulong steamId)
    {
        Racer racer = Racers.FirstOrDefault(r => r.SteamId == steamId);
        if (racer == null)
        {
            return;
        }

        Racers.Remove(racer);
    }

    public string GetLinkedRacersToString()
    {
        string result = "none";
        if (Racers.Count <= 0)
        {
            return result;
        }

        result = "";
        for (int index = 0; index < Racers.Count; index++)
        {
            Racer racer = Racers[index];
            result += racer.SteamName;
            if (index < Racers.Count - 1)
            {
                result += ", ";
            }
        }

        return result;
    }

    public string GetTag()
    {
        return $"[{Tag}]";
    }

    // New method to get the tag and name with TMP color tags
    public string GetFullColoredTagAndName()
    {
        return $"<color={Color}>{GetTag()} {Name}</color>";
    }

    public string GetColoredTag()
    {
        return $"<color={Color}>{GetTag()}</color>";
    }
}