using System.Collections.Generic;
using System.Linq;
using Showdown4.Config;
using Showdown4.Entities;

namespace Showdown4.Utils;

// Central place that knows how to load the configured teams and turn them into runtime Team
// instances. Teams are configured directly in the BepInEx config (Teams section); only when no
// team is configured there do we fall back to the legacy Teams.json file loaded via ZeepSDK's
// IModStorage (Plugin.Storage). Used by every state that needs to know about the configured teams.
public static class TeamsProvider
{
	public static List<Team> LoadTeams()
	{
		TeamData teamData = MyConfig.LoadTeams();
		if (teamData.Teams == null || teamData.Teams.Count == 0)
		{
			teamData = Plugin.Storage.LoadFromJson<TeamData>(MyConfig.TeamFileConfig.Value);
		}

		return teamData?.Teams?.Select(Team.FromJson)
			.OrderBy(team => team.GetAverageQualificationTime())
			.ToList() ?? new List<Team>();
	}

	// A team only counts as "complete" once its configured roster (ExpectedRacers, coming from
	// Teams.json / the BepInEx config) contains enough players to fill the team. We need at
	// least two complete teams to be able to set up a match at all.
	public static bool AreTeamsComplete(List<Team> teams)
	{
		return teams.Count >= 2 && teams.All(team => team.ExpectedRacers.Count >= team.MaxTeamSize);
	}
}