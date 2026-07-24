using System.Collections.Generic;
using System.Linq;

namespace Showdown4.Entities;

public class Team(string name, string tag, string color)
{
	public double QualificationTime { get; set; }

	public int Wins { get; set; }
	public int Losses { get; set; }
	public string Name { get; set; } = name;
	public string Tag { get; set; } = tag;
	public string Color { get; set; } = color;
	public int Picks { get; set; } = 1;
	public int Bans { get; set; } = 2;
	public bool MissedDraft { get; set; } = false;
	public List<Racer> Racers { get; set; } = new();

	/// <summary>
	///     The roster configured for this team in Teams.json (SteamId, name and qualification time).
	///     Used by the team linking phase to automatically link players once they are detected in the
	///     server, and to determine which team gets initiative.
	/// </summary>
	public List<Racer> ExpectedRacers { get; set; } = new();

	public int MaxTeamSize { get; set; } = 2;

	/// <summary>
	///     The synthetic "Showdown" team used to attribute auto-/random-picked maps that were not
	///     chosen by one of the real teams.
	/// </summary>
	public static Team CreateShowdownTeam()
	{
		return new Team("Showdown", "Showdown", "#ff0000");
	}

	/// <summary>
	///     Builds a runtime Team from a Teams.json entry, preserving the configured roster
	///     (<see cref="ExpectedRacers" />) and calculating the team's average qualification time.
	/// </summary>
	public static Team FromJson(TeamJsonEntry entry)
	{
		Team team = new(entry.Name, entry.Tag, entry.Color)
		{
			ExpectedRacers = (entry.Players ?? new List<TeamPlayerJsonEntry>())
				.Select(player => new Racer(player.SteamId, player.Name)
					{ QualificationTime = player.QualificationTime })
				.ToList()
		};

		team.QualificationTime = team.GetAverageQualificationTime();
		return team;
	}

	public double GetAverageQualificationTime()
	{
		return ExpectedRacers.Count > 0 ? ExpectedRacers.Average(racer => racer.QualificationTime) : 0;
	}

	public double GetCumulatedQualificationTime()
	{
		return ExpectedRacers.Sum(racer => racer.QualificationTime);
	}

	public string GetNameWithTag()
	{
		return $"{GetTag()} {Name}";
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
		return $"<{Color}>{GetTag()} {Name}</color>";
	}

	public string GetColoredTag()
	{
		return $"<{Color}>{GetTag()}</color>";
	}
}