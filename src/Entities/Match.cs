using System.Collections.Generic;
using System.Linq;
using Showdown4.Managers;
using ZeepkistNetworking;

namespace Showdown4.Entities;

public class Match
{
    public Match(Team teamA, Team teamB)
    {
        TeamA = teamA;
        TeamB = teamB;
    }

    public Team TeamA { get; set; }
    public Team TeamB { get; set; }
    public List<Round> Rounds { get; } = [];

    public List<Draft> Drafts { get; } = new List<Draft>();
    public Draft CurrentDraft => Drafts[^1];
    public Round CurrentRound => Rounds[^1];

    public string Score()
    {
        return $"{TeamA.GetTag()} {TeamA.Wins}:{TeamB.Wins} {TeamB.GetTag()}";
    }

    public void AddRound(Round round)
    {
        Rounds.Add(round);
    }

    public void AddDraft()
    {
        List<OnlineZeeplevel> unAvaiableLevels = new List<OnlineZeeplevel>();
        List<OnlineZeeplevel> allLevels = PlaylistManager.GetPlaylistLevels(Plugin.CompetitionLevelsPlaylistName.Value);
        if (Drafts.Count > 0)
        {
            unAvaiableLevels = new List<OnlineZeeplevel>(CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));
        }

        Draft draft = new Draft(TeamA, TeamB, allLevels, unAvaiableLevels);
        Drafts.Add(draft);
    }

    public int RoundCounter()
    {
        return Rounds.Count;
    }
}