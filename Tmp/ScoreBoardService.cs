// namespace Showdown4.Tmp;
//
// public class ScoreBoardService
// {
//     public void UpdateRacingScoreboard(List<Team> teams)
//     {
//         RoundEvaluator roundEvaluator = RoundEvaluator;
//         double winningTime = roundEvaluator.GetAverageTime(teams[0]);
//         double losingTime = roundEvaluator.GetAverageTime(teams[1]);
//         string remainingTimeToWinningTeam = $"{MessageFormatter.FormatTimestampDifference(winningTime - losingTime)}";
//
//
//         List<List<string>> rows = [];
//
//         List<string> cols = [];
//         // cols.Add("Pos");
//         // cols.Add("Teams");
//         // cols.Add($"Finishers");
//         // cols.Add($"Avg Time");
//         // cols.Add("Time Diff");
//         // cols.Add("#");
//         // rows.Add(cols);
//
//         for (int index = 0; index < teams.Count; index++)
//         {
//             cols = [];
//             Team team = teams[index];
//             cols.Add($"#{index + 1}:");
//             cols.Add(team.GetNameWithTag());
//             cols.Add($": ({roundEvaluator.CountFinishers(team)}/2)");
//             cols.Add($": {MessageFormatter.FormatTimestamp(roundEvaluator.GetAverageTime(team))}");
//             cols.Add(index < 1 ? "" : remainingTimeToWinningTeam);
//             cols.Add("#");
//             rows.Add(cols);
//         }
//
//         ServerMessageColor color = ColorRangeConverter.ConvertHexToColorName(teams[0].Color);
//         string formattedScoreboard = TableFormatter.FormattingScoreboard(rows);
//         LobbyController.SetServerMessage(color, formattedScoreboard);
//     }
// }

