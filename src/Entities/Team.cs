using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

	public double QualificationTime { get; set; }

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
		// string shortName = GetStringCountNoRichTags(Name) > 48 ? $"{Name[..48]}..." : Name;
		// return $"{GetTag()} {shortName}";
		return $"{GetTag()} {Name}";
	}

	public int GetStringCountNoRichTags(string str)
	{
		var result = 0;
		var openCount = 0;
		foreach (var c in str)
		{
			switch (c)
			{
				case '<':
					openCount++;
					break;
				case '>':
					openCount--;
					break;
			}

			if (openCount == 0) result++;
		}

		Debug.Log($"String {str} has {result} characters");
		return result;
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
		if (Racers.Count < MaxTeamSize) Racers.Add(racer);
	}

	public void RemoveRacer(ulong steamId)
	{
		var racer = Racers.FirstOrDefault(r => r.SteamId == steamId);
		if (racer == null) return;

		Racers.Remove(racer);
	}

	public string GetLinkedRacersToString()
	{
		var result = "none";
		if (Racers.Count <= 0) return result;

		result = "";
		for (var index = 0; index < Racers.Count; index++)
		{
			var racer = Racers[index];
			result += racer.SteamName;
			if (index < Racers.Count - 1) result += ", ";
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