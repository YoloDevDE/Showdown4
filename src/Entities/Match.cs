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
        Initiative = teamA;
    }

    public Team TeamA { get; set; }
    public Team TeamB { get; set; }
    public List<Round> Rounds { get; } = [];

    public List<Draft> Drafts { get; } = [];
    public Draft CurrentDraft => Drafts.Last();
    public Round CurrentRound => Rounds.Last();
    public Team Initiative { get; set; }
    public Team NonInitiative => Initiative == TeamA ? TeamB : TeamA;

    public string ScoreColored()
    {
        return $"<color={TeamA.Color}>{TeamA.GetTag()}</color> {TeamA.Wins}:{TeamB.Wins} <color={TeamB.Color}>{TeamB.GetTag()}</color>";
    }

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
        List<OnlineZeeplevel> allLevels = PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.CompetitionLevelsPlaylistName.Value);
        if (Drafts.Count > 0)
        {
            unAvaiableLevels = new List<OnlineZeeplevel>(CurrentDraft.PickedLevels.Select(draftAction => draftAction.Level));
        }

        Draft draft = new Draft(Initiative, NonInitiative, allLevels, unAvaiableLevels);
        Drafts.Add(draft);
    }

    public int RoundCounter()
    {
        return Rounds.Count;
    }
}