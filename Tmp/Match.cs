using System;
using System.Collections.Generic;
using System.Linq;
using ZeepkistNetworking;
using ZeepSDK.Playlist;

namespace Showdown4.Tmp;

public class Match
{
    public Match(Team teamA, Team teamB, int bestOf = 3)
    {
        TeamA = teamA;
        TeamB = teamB;
        BestOf = bestOf;
        Rounds = new List<Round>();
        ResetLevelDrafts();
    }

    public Team TeamA { get; set; }
    public Team TeamB { get; set; }
    public int BestOf { get; set; }
    public List<Round> Rounds { get; set; }

    public List<Level> Levels { get; set; }
    public HashSet<LevelDraft> LevelDrafts { get; set; }

    public Round GetCurrentRound()
    {
        return Rounds[^1];
    }

    public void ResetLevelDrafts()
    {
        LevelDrafts = new HashSet<LevelDraft>();
        PlaylistSaveJSON levelPoolPlaylist = PlaylistApi.GetPlaylist(Plugin.LevelPoolPlaylistName.Value);
        foreach (OnlineZeeplevel onlineZeeplevel in levelPoolPlaylist.levels)
        {
            LevelDrafts.Add(
                new LevelDraft
                {
                    Level = new Level(onlineZeeplevel),
                    Team = null,
                    LevelDraftType = LevelDraftType.NONE
                });
        }
    }

    public void DoBan(Team team, Level level)
    {
        // Find the level in the draft pool
        LevelDraft draft = LevelDrafts.FirstOrDefault(l => l.Level.Equals(level));

        if (draft == null)
        {
            throw new InvalidOperationException("Level not found in the draft pool.");
        }

        // Check if the level is already picked or banned
        if (draft.LevelDraftType != LevelDraftType.NONE)
        {
            throw new InvalidOperationException("Level has already been picked or banned.");
        }

        // Mark the level as banned and assign it to the banning team
        draft.LevelDraftType = LevelDraftType.BAN;
        draft.Team = team;

        // Optional: Log or send a message about the ban
        Console.WriteLine($"Team {team.Name} has banned the level: {level.OnlineZeeplevel.Name}");
    }

    public int RoundCounter()
    {
        if (Rounds == null || Rounds.Count < 1)
        {
            return 1;
        }

        return Rounds.Count;
    }
}