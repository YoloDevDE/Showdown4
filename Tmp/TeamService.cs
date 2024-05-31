using System.Collections.Generic;

namespace Showdown4.Tmp;

public class TeamService
{
    public void AddRacer(Team team, Racer racer)
    {
        if (team.Racers.Count < team.MaxTeamSize)
        {
            team.Racers.Add(racer);
        }
    }

    public string GetFormattedTeams(List<Team> teams)
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