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

    // Generate a server message for team and racer leaderboard
    public ServerMessage GenerateLeaderboardMessage(Team teamA, Team teamB)
    {
        List<Team> sortedTeams = _round.EvaluateTeamsSortedByWinner(teamA, teamB);

        ServerMessage msg = new ServerMessage()
            .ShowdownHeader()
            .AddLine(line => line.AddBlock("Leaderboard:").Bold());

        int position = 1;
        foreach (Team team in sortedTeams)
        {
            double averageTime = _round.CalculateAverageTime(team);
            int finishers = _round.CountFinishers(team);

            // Add team header with team color
            msg.AddLine(line => line
                .AddBlock($"#{position} ", f => f.Bold().Color("#ffffff"))
                .AddBlock($"{team.GetTag()}", f => f.Bold().Color(team.Color))
                .AddBlock($" - Avg Time: {averageTime.GetFormattedTime()} ", f => f.Color("#ffffff"))
                .AddBlock($"- Finishers: {finishers}/2", f => f.Color("#ffffff"))
            );

            // Add racers' individual times, making them smaller
            foreach (Racer racer in team.Racers)
            {
                double bestTime = _round.GetPersonalBest(racer);
                string timeDisplay = bestTime == double.MaxValue ? "-:--.---" : $"{bestTime.GetFormattedTime()}";

                msg.AddLine(line => line
                        .AddBlock($"   {racer.SteamName}: ",
                            f => f.Color("#ffffff").FontSize(12)) // Smaller font size for racers
                        .AddBlock($"{timeDisplay}", f => f.Color("#ffffff").FontSize(12)) // Smaller font size for times
                );
            }

            position++;
        }

        return msg;
    }
}