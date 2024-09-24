using System;
using System.Collections.Generic;
using Showdown4.Entities;
using ZeepkistNetworking;

namespace Showdown4.States.Showdown;

public class Draft
{
    private readonly Team teamA;
    private readonly Team teamB;
    private Team currentTeam;

    public Draft(Team teamA, Team teamB, List<OnlineZeeplevel> levels)
    {
        AvailableLevels = new List<OnlineZeeplevel>(levels); // Copy levels
        AllLevels = new List<OnlineZeeplevel>(levels); // Copy levels
        this.teamA = teamA;
        this.teamB = teamB;
        currentTeam = teamA; // For example, Team A starts
    }

    private Team otherTeam => currentTeam.Equals(teamA) ? teamB : teamA;


    public List<OnlineZeeplevel> AllLevels { get; }
    public List<OnlineZeeplevel> AvailableLevels { get; } // Levels available to pick/ban
    public List<OnlineZeeplevel> PickedLevels { get; } = new List<OnlineZeeplevel>();
    public List<OnlineZeeplevel> BannedLevels { get; } = new List<OnlineZeeplevel>();

    public bool ForcePick { get; set; }

    public void PickLevel(OnlineZeeplevel level)
    {
        if (currentTeam.Picks <= 0)
        {
            throw new InvalidOperationException("You have no more picks left.");
        }

        if (!AvailableLevels.Contains(level))
        {
            throw new InvalidOperationException("Level is not available anymore");
        }

        ForcePick = true;
        PickedLevels.Add(level);
        AvailableLevels.Remove(level);
        currentTeam.Picks -= 1;
        SwitchTeam();
    }

    public void BanLevel(OnlineZeeplevel level)
    {
        if (ForcePick)
        {
            throw new InvalidOperationException($"{otherTeam.GetTag()} has picked a level and therefore you can't ban a level anymore.");
        }

        if (currentTeam.Bans <= 0)
        {
            throw new InvalidOperationException("You have no more bans left.");
        }

        if (!AvailableLevels.Contains(level))
        {
            throw new InvalidOperationException("Level is not available anymore");
        }

        BannedLevels.Add(level);
        AvailableLevels.Remove(level);
        currentTeam.Bans -= 1;
        SwitchTeam();
    }

    public bool IsDraftComplete()
    {
        // Both teams have no picks and no bans left
        bool bothTeamsOutOfPicksAndBans = teamA.Picks == 0 && teamA.Bans == 0 && teamB.Picks == 0 && teamB.Bans == 0;

        // Two maps have been picked
        bool twoMapsPicked = PickedLevels.Count >= 2;

        // Draft is complete if either of these conditions is true
        return bothTeamsOutOfPicksAndBans || twoMapsPicked;
    }

    public void SwitchTeam()
    {
        currentTeam = otherTeam; // Switch turn
    }

    public Team GetCurrentTeam()
    {
        return currentTeam;
    }

    public Team GetOtherTeam()
    {
        return otherTeam;
    }
}