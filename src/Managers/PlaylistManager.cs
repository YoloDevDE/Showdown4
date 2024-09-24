using System;
using System.Collections.Generic;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Multiplayer;
using ZeepSDK.Playlist;

namespace Showdown4.Managers;

public abstract class PlaylistManager
{
    public static int GetCurrentPlaylistIndex()
    {
        return ZeepkistNetwork.CurrentLobby.CurrentPlaylistIndex;
    }

    public static OnlineZeeplevel GetCurrentPlaylistLevel()
    {
        return ZeepkistNetwork.CurrentLobby.Playlist[GetCurrentPlaylistIndex()];
    }

    public static void SetServerPlaylist(List<OnlineZeeplevel> playlist)
    {
        ZeepkistNetwork.CurrentLobby.Playlist = playlist;
        MultiplayerApi.UpdateServerPlaylist();
    }

    public static void SetServerPlaylist(string playlistName)
    {
        if (PlaylistApi.Exists(playlistName))
        {
            PlaylistSaveJSON playlist = PlaylistApi.GetPlaylist(playlistName);
            SetServerPlaylist(playlist.levels);
        }
        else
        {
            throw new InvalidOperationException("Playlist does not exist");
        }
    }
}