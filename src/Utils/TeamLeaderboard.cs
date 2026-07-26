using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;

namespace Showdown4.Utils;

public class TeamLeaderboard(Round round)
{
	// Tracks the previous team order so we can show position-change arrows
	private readonly Dictionary<string, int> _previousPositions = new();

	// Generate a server message for team and racer leaderboard
	public ServerMessage GenerateLeaderboardMessage(Match match, bool racingColors = false)
	{
		round.Evaluate(out List<Team> teams);
		List<Team> sortedTeams = round.GetTeamsSortedByWinnerAsc();

		ServerMessage msg = new ServerMessage()
			.ShowdownHeader(true)
			.AddLine(l => l.Size(30).Bold().AddBlock(match.ScoreColored()).Indent("585%"));
		// Build new positions map and compute arrows before rendering
		Dictionary<string, int> newPositions = new();
		for (int i = 0; i < sortedTeams.Count; i++)
			newPositions[sortedTeams[i].GetTag()] = i + 1;

		for (int i = 0; i < sortedTeams.Count; i++)
		{
			int position = i + 1;
			Team team = sortedTeams[i];
			double averageTime = round.GetAvgTimeOfTeam(team);
			int finishers = round.GetFinishersCount(team);

			string tag = team.GetTag();
			string arrow = "";
			if (_previousPositions.TryGetValue(tag, out int prevPos) && prevPos != position)
			{
				arrow = prevPos > position ? " ^" : " v";
			}

			msg.AddLine(line =>
			{
				line.Size(25)
					.AddBlock($"#{position}{arrow}",
						f => f.Bold().Color(racingColors ? i == 0 ? "#00ff00" : "#ff0000" :
							position == 1 ? "#FFD700" : "#C0C0C0"))
					.AddBlock($"{team.GetTag()}".PadRight(6), f => f.Bold().Color(team.Color));


				List<Racer> sortedRacers = team.Racers
					.Where(racer => round.Leaderboard.ContainsKey(racer.SteamId))
					.OrderBy(racer => round.GetPersonalBest(racer))
					.ToList();


				switch (round.WinningMethod)
				{
					case WinningMethod.Finishers:


						line.AddBlock($"Finishers: {finishers}/2", f => f.Color("#ffffff"));

						break;

					case WinningMethod.CumulativeTime:
						// Show the average team time if the winner was determined by cumulative time
						line.AddBlock($"Time: {averageTime.GetFormattedTime()} ", f => f.Color("#ffffff"));
						if (position > 1)
						{
							line.AddBlock(
								$"+{(round.GetAvgTimeOfTeam(sortedTeams[1]) - round.GetAvgTimeOfTeam(sortedTeams[0])).GetFormattedTime()}",
								f => f.Color("#ffff00"));
						}

						break;

					case WinningMethod.IndividualPlacements:


						line.AddBlock("Individual Placements: ", f => f.Color("#ffffff"));

						for (int j = 0; j < sortedRacers.Count; j++)
						{
							line.AddBlock(
								$"{sortedRacers[j].SteamName} ({round.GetPersonalBest(sortedRacers[j]).GetFormattedTime()})",
								f => f.Color("#ffffff"));
							if (j < sortedRacers.Count - 1)
							{
								line.AddBlock(", ", f => f.Color("#ffffff"));
							}
						}

						break;

					case WinningMethod.RandomSelection:

						if (round.GetFinishersCount(round.TeamB) + round.GetFinishersCount(round.TeamA) <
						    round.GetTeamsSortedByWinnerAsc()[0].Racers.Count +
						    round.GetTeamsSortedByWinnerAsc()[1].Racers.Count)
						{
							line.AddBlock($"Finishers: {finishers}/2", f => f.Color("#ffffff"));
						}
						else
						{
							line.AddBlock($"Time: {averageTime.GetFormattedTime()} ", f => f.Color("#ffffff"));
							if (position > 1)
							{
								line.AddBlock(
									$"={(round.GetAvgTimeOfTeam(sortedTeams[1]) - round.GetAvgTimeOfTeam(sortedTeams[0])).GetFormattedTime()}",
									f => f.Color("#f7dcaa"));
							}
						}

						break;
				}
			});
		}

		// Update cached positions for next call
		_previousPositions.Clear();
		foreach (KeyValuePair<string, int> kv in newPositions)
			_previousPositions[kv.Key] = kv.Value;

		return msg;
	}
}