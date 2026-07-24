using System;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Config;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Level;
using ZeepSDK.Multiplayer;
using ZeepSDK.Playlist;

namespace Showdown4.Managers;

public abstract class PlaylistManager
{
	/// <summary>
	///     Whether the currently loaded level is the intermission level configured via
	///     <see cref="MyConfig.IntermissionLevelPlaylistNameConfig" />.
	/// </summary>
	public static bool IsOnIntermissionLevel()
	{
		return LevelApi.CurrentLevel.UID.Equals(
			GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)[0].UID);
	}

	public static void ResetPlaylist()
	{
		OnlineZeeplevel intermissionLevel =
			GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value).First();
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