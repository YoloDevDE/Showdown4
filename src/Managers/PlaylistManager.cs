using System;
using System.Collections.Generic;
using System.Linq;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Multiplayer;
using ZeepSDK.Playlist;

namespace Showdown4.Managers;

public abstract class PlaylistManager
{
    public static void ResetPlaylist()
    {
        OnlineZeeplevel intermissionLevel = GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value).First();
        ZeepkistNetwork.CurrentLobby.Playlist.Clear();
        ZeepkistNetwork.CurrentLobby.Playlist.Add(intermissionLevel);
        ZeepkistNetwork.CurrentLobby.PlaylistRandom = false;
        ZeepkistNetwork.CurrentLobby.CurrentPlaylistIndex = 0;
        MultiplayerApi.SetNextLevelIndex(0);
    }


    public static void SetServerPlaylist(List<OnlineZeeplevel> playlist)
    {
        ZeepkistNetwork.CurrentLobby.Playlist.Clear();
        ZeepkistNetwork.CurrentLobby.Playlist.AddRange(playlist);
        ZeepkistNetwork.CurrentLobby.PlaylistRandom = false;
        ZeepkistNetwork.CurrentLobby.CurrentPlaylistIndex = 0;
        MultiplayerApi.SetNextLevelIndex(0);
    }


    public static List<OnlineZeeplevel> GetLocalLevelsByPlaylistName(string playlistName)
    {
        return PlaylistApi.GetPlaylist(playlistName).levels;
    }

    public static void SetServerPlaylist(string playlistName)
    {
        if (PlaylistApi.Exists(playlistName))
        {
            SetServerPlaylist(GetLocalLevelsByPlaylistName(playlistName));
        }
        else
        {
            throw new InvalidOperationException("Playlist does not exist");
        }
    }
}