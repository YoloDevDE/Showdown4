using System.Collections.Generic;
using Showdown4.Entities;

namespace Showdown4.Utils;

public class Leaderboard
{
    private readonly Round _round;

    public Leaderboard(Round round)
    {
        _round = round;
    }

    public ServerMessage GenerateDefaultLeaderboardMessage(Team teamA, Team teamB)
    {
        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Current Leaderboard:").Bold());

        // Default entry for Team A
        msg.AddLine(line => line
            .AddBlock($"{teamA.GetTag()}", f => f.Color(teamA.Color).Bold())
            .AddBlock(" - Avg Time: ")
            .AddBlock("-:--.---", f => f.Color("#ffffff"))
            .AddBlock(" - Finishers: 0/2"));

        // Default entry for Team B
        msg.AddLine(line => line
            .AddBlock($"{teamB.GetTag()}", f => f.Color(teamB.Color).Bold())
            .AddBlock(" - Avg Time: ")
            .AddBlock("-:--.---", f => f.Color("#ffffff"))
            .AddBlock(" - Finishers: 0/2"));

        return msg;
    }

    // Generate a server message for team and racer leaderboard
    public ServerMessage GenerateLeaderboardMessage()
    {
        _round.Evaluate(out List<Team> teams);
        List<Team> sortedTeams = _round.GetTeamsSortedByWinnerAsc();

        ServerMessage msg = new ServerMessage()
            .ShowdownHeader();

        int position = 1;
        foreach (Team team in sortedTeams)
        {
            double averageTime = _round.GetAvgTimeOfTeam(team);
            int finishers = _round.GetFinishersCount(team);

            // Check if there are 4 finishers across both teams
            int totalFinishers = _round.GetFinishersCount(_round.teamA) + _round.GetFinishersCount(_round.teamA);
            msg.AddLine(line =>
            {
                line
                    .AddBlock($"#{position} ", f => f.Bold().Color("#ffffff"))
                    .AddBlock($"{team.GetTag()}", f => f.Bold().Color(team.Color));


                // If fewer than 4 finishers, show only the number of finishers, otherwise show the average time
                if (totalFinishers < 4)
                {
                    line.AddBlock($"| Finishers: {finishers}/2", f => f.Color("#ffffff")
                    );
                }
                else
                {
                    line.AddBlock($"| Avg Time: {averageTime.GetFormattedTime()} ", f => f.Color("#ffffff")
                    );
                    if (position > 1)

                    {
                        line.AddBlock($"+{(_round.GetAvgTimeOfTeam(sortedTeams[1]) - _round.GetAvgTimeOfTeam(sortedTeams[0])).GetFormattedTime()}", f => f.Color("#ffff00"));
                    }
                }
            });

            position++;
        }

        return msg;
    }
}