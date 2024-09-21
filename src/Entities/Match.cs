using System;
using System.Collections.Generic;
using System.Linq;
using ZeepkistNetworking;
using ZeepSDK.Playlist;

namespace Showdown4.Entities;

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

    public bool PickWasUsed { get; set; }
    public List<Round> Rounds { get; set; }

    public List<Level> Levels { get; set; }
    public HashSet<LevelDraft> LevelDrafts { get; set; }

    public Round GetCurrentRound()
    {
        return Rounds[^1];
    }

    public void ResetLevelDrafts()
    {
        // Check if the playlist exists
        string playlistName = Plugin.LevelPoolPlaylistName.Value;
        if (!PlaylistApi.Exists(playlistName))
            throw new InvalidOperationException($"Playlist '{playlistName}' does not exist.");

        // Fetch the playlist
        PlaylistSaveJSON levelPoolPlaylist = PlaylistApi.GetPlaylist(playlistName);

        LevelDrafts = new HashSet<LevelDraft>();
        foreach (OnlineZeeplevel onlineZeeplevel in levelPoolPlaylist.levels)
            LevelDrafts.Add(
                new LevelDraft
                {
                    Level = new Level(onlineZeeplevel),
                    Team = null,
                    LevelDraftType = LevelDraftType.NONE
                });
    }

    public void DoPick(Team team, Level level)
    {
        // Find the level in the draft pool
        LevelDraft draft = LevelDrafts.FirstOrDefault(l => l.Level.Equals(level));
        if (team.Picks == 0) throw new InvalidOperationException("Your Team has no more picks left. Nerd");

        if (draft == null) throw new InvalidOperationException("Level not found in the draft pool. Nerd");

        // Check if the level is already picked or banned
        if (draft.LevelDraftType != LevelDraftType.NONE)
            throw new InvalidOperationException("Level has already been picked or banned. Nerd");

        // Mark the level as picked and assign it to the picking team
        draft.LevelDraftType = LevelDraftType.PICK;
        draft.Team = team;
        team.Picks--;
        PickWasUsed = true;
        Console.WriteLine($"Team {team.Name} has picked the level: {level.OnlineZeeplevel.Name}");
    }

    public void DoBan(Team team, Level level)
    {
        // Find the level in the draft pool
        LevelDraft draft = LevelDrafts.FirstOrDefault(l => l.Level.Equals(level));
        if (PickWasUsed)
            throw new InvalidOperationException(
                "You need to pick a level now because the enemy nerds used a pick. Nerd");

        if (team.Bans == 0) throw new InvalidOperationException("Your Team has no more bans left. Nerd");

        if (draft == null) throw new InvalidOperationException("Level not found in the draft pool. Nerd");

        // Check if the level is already picked or banned
        if (draft.LevelDraftType != LevelDraftType.NONE)
            throw new InvalidOperationException("Level has already been picked or banned. Nerd");

        // Mark the level as banned and assign it to the banning team
        draft.LevelDraftType = LevelDraftType.BAN;
        draft.Team = team;
        team.Bans--;
        Console.WriteLine($"Team {team.Name} has banned the level: {level.OnlineZeeplevel.Name}");
    }

    public int RoundCounter()
    {
        if (Rounds == null || Rounds.Count < 1) return 1;

        return Rounds.Count;
    }
}