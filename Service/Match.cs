using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;
using Showdown4.Utils;

namespace Showdown4.Service;

public class Match
{
    private readonly int _raceWinsNeededForMatchWin = 2;
    public List<Round> Rounds { get; } = new List<Round>();
    public Team TeamA { get; set; }
    public Team TeamB { get; set; }

    public Team Initiative { get; set; }

    public List<Team> RoundWins { get; set; }
    public List<Team> RoundLosses { get; set; }
    public Team CurrentRoundWinner { get; set; }
    public Team CurrentRoundLoser { get; set; }
    public Team MatchWinner { get; set; }
    public Team MatchLoser { get; set; }

    public Round CurrentRound => Rounds.Last();

    public int RoundCounter { get; set; } = 1;

    public Team EvaluateMatchWinner()
    {
        RoundCounter++;
        CurrentRoundWinner.Wins++;
        CurrentRoundLoser.Losses++;
        if (TeamA.Wins >= _raceWinsNeededForMatchWin)
        {
            MatchLoser = TeamB;
            return MatchWinner = TeamA;
        }

        if (TeamB.Wins >= _raceWinsNeededForMatchWin)
        {
            MatchLoser = TeamA;
            return MatchWinner = TeamB;
        }

        return null;
    }


    public void UpdateRacingScoreboard(Team winningTeam, Team losingTeam)
    {
        RoundEvaluator roundEvaluator = new RoundEvaluator(winningTeam, losingTeam, CurrentRound);
        string winningTeamFinisherCount = $": ({roundEvaluator.GetFinisherCount(winningTeam)}/2)";
        string losingTeamFinisherCount = $": ({roundEvaluator.GetFinisherCount(losingTeam)}/2)";

        string keyWinning = $"{winningTeam.GetNameWithTag()}";
        string keyLosing = $"{losingTeam.GetNameWithTag()}";
        double winningTime = roundEvaluator.GetAverageTime(winningTeam);
        double losingTime = roundEvaluator.GetAverageTime(losingTeam);
        string valueWinning = $": {MessageFormatter.FormatTimestamp(winningTime)}";
        string valueLosing = $": {MessageFormatter.FormatTimestamp(losingTime)}";
        string valueTimeDiff = $"{MessageFormatter.FormatTimestampDifference(winningTime - losingTime)}";

        List<string> line1 = new List<string> { keyWinning, winningTeamFinisherCount, valueWinning, "#" };
        List<string> line2 = new List<string> { keyLosing, losingTeamFinisherCount, valueLosing + " " + $"({valueTimeDiff})", "#" };

        string result = TableFormatter.AlignTableColumns(new List<List<string>> { line1, line2 });
        ServerMessageColor color = ColorRangeConverter.ConvertHexToColorName(winningTeam.Color);
        ChatUtils.SetServerMessage(color, result);
    }

    public void ResetResults()
    {
    }
}