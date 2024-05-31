using System.Collections.Generic;

namespace Showdown4.Tmp;

public class Round
{
    public Round(int roundNumber)
    {
        RoundNumber = roundNumber;
        Leaderboard = new Dictionary<ulong, double>();
    }

    public int RoundNumber { get; set; }
    public Dictionary<ulong, double> Leaderboard { get; set; } // Dictionary<SteamId, BestTime>

    public void AddResult(Result result)
    {
        if (Leaderboard.ContainsKey(result.Racer.SteamId))
        {
            // Update the best time if the new time is better
            if (result.Time < Leaderboard[result.Racer.SteamId])
            {
                Leaderboard[result.Racer.SteamId] = result.Time;
            }
        }
        else
        {
            // Add new racer and their time
            Leaderboard[result.Racer.SteamId] = result.Time;
        }
    }

    public int GetNumberOfFinishers()
    {
        return Leaderboard.Count;
    }

    public double GetRacerBestTime(ulong steamId)
    {
        return Leaderboard.ContainsKey(steamId) ? Leaderboard[steamId] : double.MaxValue;
    }
}