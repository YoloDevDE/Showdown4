using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;
using ZeepkistNetworking;

public class Draft
{
    private readonly Team teamA;
    private readonly Team teamB;
    private Team currentTeam;

    public Draft(Team teamA, Team teamB, List<OnlineZeeplevel> allLevels, List<OnlineZeeplevel> unAvailableLevels = null)
    {
        AllLevels = new List<OnlineZeeplevel>(allLevels);

        // Initialize unavailable levels as an empty list if it's null
        if (unAvailableLevels == null)
        {
            unAvailableLevels = new List<OnlineZeeplevel>();
        }

        // Make a copy of all levels as available levels
        AvailableLevels = new List<OnlineZeeplevel>(AllLevels);

        // Remove all unavailable levels from available levels by comparing their UID
        AvailableLevels.RemoveAll(level => unAvailableLevels.Any(unavailable => unavailable.UID == level.UID));

        UnAvailableLevels = new List<OnlineZeeplevel>(unAvailableLevels);

        this.teamA = teamA;
        this.teamB = teamB;
        currentTeam = teamA;
    }


    private Team otherTeam => currentTeam.Equals(teamA) ? teamB : teamA;

    public List<OnlineZeeplevel> AllLevels { get; }
    public List<OnlineZeeplevel> AvailableLevels { get; }
    public List<OnlineZeeplevel> UnAvailableLevels { get; }
    public List<DraftAction> PickedLevels { get; } = new List<DraftAction>();
    public List<DraftAction> BannedLevels { get; } = new List<DraftAction>();

    public bool IsPickPhase { get; set; }

    public void PickLevel(OnlineZeeplevel level)
    {
        if (currentTeam.Picks <= 0)
        {
            throw new InvalidOperationException("You have no more picks left.");
        }

        if (!AvailableLevels.Any(l => l.UID.Equals(level.UID)) || UnAvailableLevels.Any(l => l.UID.Equals(level.UID)))
        {
            throw new InvalidOperationException("Level is not available anymore");
        }

        IsPickPhase = true;
        PickedLevels.Add(new DraftAction(level, currentTeam, true));
        AvailableLevels.Remove(level);
        currentTeam.Picks -= 1;
        SwitchTeam();
    }

    public void BanLevel(OnlineZeeplevel level)
    {
        if (IsPickPhase)
        {
            throw new InvalidOperationException($"{otherTeam.GetTag()} has picked a level and therefore you can't ban a level anymore.");
        }

        if (currentTeam.Bans <= 0)
        {
            throw new InvalidOperationException("You have no more bans left.");
        }

        if (!AvailableLevels.Any(l => l.UID.Equals(level.UID)) || UnAvailableLevels.Any(l => l.UID.Equals(level.UID)))
        {
            throw new InvalidOperationException("Level is not available anymore");
        }

        BannedLevels.Add(new DraftAction(level, currentTeam, false));
        AvailableLevels.Remove(level);
        currentTeam.Bans -= 1;
        SwitchTeam();
    }

    public bool IsDraftComplete()
    {
        bool noMoreInventoryLeft = teamA.Bans + teamB.Bans == 0;
        bool noPicksLeft = teamA.Picks + teamB.Picks == 0 && IsPickPhase;

        return noPicksLeft || noMoreInventoryLeft;
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