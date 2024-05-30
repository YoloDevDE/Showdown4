using System.Collections.Generic;
using Showdown4.Entities;

namespace Showdown4.Service;

public class TeamService
{
    public static string GetFormattedTeams(List<Team> teams)
    {
        string result = "";
        for (int index = 0; index < teams.Count; index++)
        {
            Team team = teams[index];
            result += index + " : " + team.Tag;
            if (index < teams.Count - 1)
            {
                result += "<br>";
            }
        }

        return result;
    }
}