using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;
using ZeepkistClient;

namespace Showdown4.Managers;

/// <summary>
///     Central place for the "reset everyone" operations that happen between two racing sub-rounds:
///     wiping the leaderboard entry of every racer, respawning them and blocking/unblocking who is
///     allowed to set a time.
///     Everything goes through the custom leaderboard API of <see cref="ZeepkistNetwork" />. The chat
///     command '/resettime' must never be used for this: it resets the <b>lobby timer</b> to the round
///     length of the playlist and would therefore destroy our "+24h" timer trick.
/// </summary>
public static class RacerResetService
{
	// Respawns the given players (they are put back at the start line).
	public static void RespawnPlayers(IEnumerable<ulong> steamIds)
	{
		List<ulong> ids = steamIds.Distinct().ToList();
		if (ids.Count == 0)
		{
			return;
		}

		ZeepkistNetwork.CustomLeaderBoard_ResetPlayers(ids);
	}

	// Respawns everyone who is currently in the lobby.
	public static void RespawnEveryone()
	{
		RespawnPlayers(GetEveryoneInLobby());
	}

	// Removes the leaderboard entry of the given players and drops every display override we set
	// during the sub-round (time, name, position), so the next sub-round starts on a completely empty
	// leaderboard instead of still showing the times of the previous one.
	public static void ClearLeaderboard(IEnumerable<ulong> steamIds)
	{
		foreach (ulong steamId in steamIds.Distinct())
		{
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(steamId, null, null, null, null, null);
			ZeepkistNetwork.CustomLeaderBoard_RemovePlayerFromLeaderboard(steamId, false);
		}
	}

	// Removes the leaderboard entry of everyone in the lobby.
	public static void ClearLeaderboardForEveryone()
	{
		ClearLeaderboard(GetEveryoneInLobby());
	}

	public static void BlockEveryoneFromSettingTime()
	{
		ZeepkistNetwork.CustomLeaderBoard_BlockEveryoneFromSettingTime(true);
	}

	public static void UnblockEveryoneFromSettingTime()
	{
		ZeepkistNetwork.CustomLeaderBoard_UnblockEveryoneFromSettingTime(true);
	}

	// Everyone who is not part of one of the two competing teams is blocked from setting a time, so
	// spectators can never pollute the leaderboard of a sub-round.
	public static void BlockNonRacers(Team teamA, Team teamB)
	{
		foreach (ZeepkistNetworkPlayer player in ZeepkistNetwork.PlayerList.Where(player =>
			         !teamA.Racers.Exists(racer => racer.SteamId == player.SteamID) &&
			         !teamB.Racers.Exists(racer => racer.SteamId == player.SteamID)))
			ZeepkistNetwork.CustomLeaderBoard_BlockPlayerFromSettingTime(player.SteamID, true);
	}

	// The steam ids of every racer of both competing teams.
	public static List<ulong> GetRacerSteamIds(Team teamA, Team teamB)
	{
		return teamA.Racers.Concat(teamB.Racers).Select(racer => racer.SteamId).Distinct().ToList();
	}

	private static List<ulong> GetEveryoneInLobby()
	{
		return ZeepkistNetwork.PlayerList.Select(player => player.SteamID).ToList();
	}
}