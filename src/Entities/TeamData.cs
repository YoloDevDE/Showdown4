using System.Collections.Generic;

namespace Showdown4.Entities;

// Root object of the Teams.json file. It intentionally does not map directly to the
// runtime Team/Racer entities, since the JSON uses "players" (with a string SteamId
// and a per-player qualification time) instead of the runtime's "Racers".
public class TeamData
{
	public List<TeamJsonEntry> Teams { get; set; }
}

public class TeamJsonEntry
{
	public string Name { get; set; }
	public string Tag { get; set; }
	public string Color { get; set; }
	public List<TeamPlayerJsonEntry> Players { get; set; } = new();
}

public class TeamPlayerJsonEntry
{
	public string Name { get; set; }
	public ulong SteamId { get; set; }
	public double QualificationTime { get; set; }
}