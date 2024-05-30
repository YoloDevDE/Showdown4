using System.Collections.Generic;

namespace Showdown4.Entities;

public class TeamService
{
    public List<Team> Teams { get; set; }

    public string GetFormattedTeams(List<Team> teams) {

        var result = "";
        for (var index = 0; index < teams.Count; index++)
        {
            var team = teams[index];
            result += index + " : " + team.Tag;
            if (index < teams.Count - 1)
                result += "<br>";
        }

        return result;
    }
}