using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;
using ZeepkistClient;
using ZeepkistNetworking;

namespace Showdown4.Utils;

/// <summary>
///     Manages the CustomLeaderboard display during the drafting phase.
///     On activation it caches every racer's current leaderboard time, clears all racer
///     entries, and populates four synthetic slots (positions 1+2 for the active team,
///     3+4 for the waiting team) with coloured overrides that show who is currently
///     drafting and who is waiting.  On deactivation it removes the overrides and
///     restores the original times.
/// </summary>
public class DraftingLeaderboard
{
	// Colours used for the two status strings
	private const string DraftingColor = "#00cc44";
	private const string WaitingColor = "#888888";

	// What replaces the position number for the waiting team
	private const string WaitingPositionText = "WAIT";

	// Cached times per SteamID so we can restore them later
	private readonly Dictionary<ulong, float> _cachedTimes = new();

	private readonly Team _teamA;
	private readonly Team _teamB;

	// Countdown shown in the position column for slot 0 of the active team (updated every tick)
	private int _currentCountdown;

	public DraftingLeaderboard(Team teamA, Team teamB)
	{
		_teamA = teamA;
		_teamB = teamB;
	}

	/// <summary>
	///     Caches the current leaderboard times for all racers, removes them from the
	///     board, then populates the four drafting slots for the given active team.
	/// </summary>
	public void Activate(Team activeTeam, int countdown)
	{
		_currentCountdown = countdown;
		CacheAndClearRacers();
		PopulateLeaderboard(activeTeam);
	}

	/// <summary>
	///     Updates the four leaderboard slots when the active team changes (i.e. after
	///     a pick/ban/pass moves the turn to the other team).
	/// </summary>
	public void UpdateActiveTeam(Team activeTeam, int countdown)
	{
		_currentCountdown = countdown;
		PopulateLeaderboard(activeTeam);
	}

	/// <summary>
	///     Updates the countdown shown in the active team's position column without
	///     changing which team is active.
	/// </summary>
	public void UpdateCountdown(Team activeTeam, int countdown)
	{
		_currentCountdown = countdown;
		PopulateLeaderboard(activeTeam);
	}

	/// <summary>
	///     Flashes all leaderboard entries white by clearing all overrides for 0.25 s.
	///     The caller is responsible for restoring normal overrides afterwards via
	///     <see cref="UpdateCountdown" /> or <see cref="UpdateActiveTeam" />.
	/// </summary>
	public void FlashWhite()
	{
		IEnumerable<Racer> allRacers = _teamA.Racers.Concat(_teamB.Racers);
		foreach (Racer racer in allRacers)
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(
				racer.SteamId, null, null, null, null, null);
	}

	/// <summary>
	///     Removes all overrides and restores each racer's original leaderboard time.
	///     Call this when leaving the drafting phase.
	/// </summary>
	public void Deactivate()
	{
		RestoreRacers();
	}

	// ── private helpers ──────────────────────────────────────────────────────

	private void CacheAndClearRacers()
	{
		_cachedTimes.Clear();

		List<LeaderboardItem> leaderboard = ZeepkistNetwork.Leaderboard.ToList();

		IEnumerable<Racer> allRacers = _teamA.Racers.Concat(_teamB.Racers);
		foreach (Racer racer in allRacers)
		{
			// Look up the player's current time on the live leaderboard
			// (LeaderboardItem is a struct, so we can't compare with null)
			if (leaderboard.Any(l => l.SteamID == racer.SteamId))
			{
				_cachedTimes[racer.SteamId] = leaderboard.First(l => l.SteamID == racer.SteamId).Time;
			}

			// Remove the player from the leaderboard (no notification)
			ZeepkistNetwork.CustomLeaderBoard_RemovePlayerFromLeaderboard(racer.SteamId, false);
		}
	}

	private void PopulateLeaderboard(Team activeTeam)
	{
		Team waitingTeam = activeTeam == _teamA ? _teamB : _teamA;

		// Active team gets times 1.0 / 2.0  → positions 1 & 2
		// Waiting team gets times 3.0 / 4.0 → positions 3 & 4
		SetTeamSlots(activeTeam, 1.0f, true, _currentCountdown);
		SetTeamSlots(waitingTeam, 3.0f, false, 0);
	}

	private static void SetTeamSlots(Team team, float startTime, bool isDrafting, int countdown)
	{
		string timeText = isDrafting
			? $"<{DraftingColor}>Currently drafting</color>"
			: $"<{WaitingColor}>Waiting...</color>";

		for (int i = 0; i < team.Racers.Count && i < 2; i++)
		{
			Racer racer = team.Racers[i];
			float slotTime = startTime + i;
			string nameText = BuildNameOverride(team, racer);

			// Slot 0 of the active team shows the live draft countdown in the position column
			string posText = isDrafting && i == 0
				? TimeFormatter.FormatDuration(countdown)
				: WaitingPositionText;

			// Place the player on the leaderboard with a synthetic time that
			// ensures the correct sort order (no player notification).
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerTimeOnLeaderboard(racer.SteamId, slotTime, false);

			// Override all visible columns
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(
				racer.SteamId,
				timeText, // time column
				nameText, // name column
				posText, // position column (replaces "1", "2", …)
				null,
				null);
		}
	}

	private void RestoreRacers()
	{
		IEnumerable<Racer> allRacers = _teamA.Racers.Concat(_teamB.Racers);
		foreach (Racer racer in allRacers)
		{
			// Clear all overrides first (empty strings reset to defaults)
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(
				racer.SteamId, null, null, null, null, null);

			if (_cachedTimes.TryGetValue(racer.SteamId, out float originalTime))
			{
				// Restore the player's real time (no notification)
				ZeepkistNetwork.CustomLeaderBoard_SetPlayerTimeOnLeaderboard(
					racer.SteamId, originalTime, false);
			}
			else
			{
				// Player had no time before – remove them again so the board is clean
				ZeepkistNetwork.CustomLeaderBoard_RemovePlayerFromLeaderboard(racer.SteamId, false);
			}
		}
	}

	private static string BuildNameOverride(Team team, Racer racer)
	{
		return $"<nobr><{team.Color}>{team.GetTag()} {racer.SteamName}</color></nobr>";
	}
}