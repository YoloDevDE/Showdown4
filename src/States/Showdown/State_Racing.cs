using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class StateRacing(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// Accumulates how many positions a racer has lost since their override was last reset,
	// so multiple losses in quick succession are shown as a single combined number.
	private readonly Dictionary<ulong, int> _lostAccum = new();
	private readonly Dictionary<ulong, (string Color, string Name)> _playerInfo = new();

	// Tracks the leaderboard position/time/display info of every racer that has already set
	// a time, so subsequent results can be compared against them.
	private readonly Dictionary<ulong, int> _playerPositions = new();
	private readonly Dictionary<ulong, double> _playerTimes = new();

	// The pending "revert to normal" coroutine per racer, so a new position change can
	// restart/replace the countdown instead of stacking multiple resets.
	private readonly Dictionary<ulong, Coroutine> _resetCoroutines = new();

	private Round _currentRound;

	private Team _teamA, _teamB;
	private TeamLeaderboard _teamLeaderboard;

	// Delay before a gained/lost/equal/new position override is reverted back to the
	// normal time+name display, and the colors/symbols used for it, are all user-configurable.
	// Mirrors the "resetDelay"/"gained"/"lost"/"equal" config from the reference implementation.
	private static float ResetDelay => MyConfig.LeaderboardOverrideResetDelayConfig.Value;
	private static string GainedArrow => MyConfig.LeaderboardGainedArrowConfig.Value;
	private static string GainedColor => MyConfig.LeaderboardGainedColorConfig.Value;
	private static string LostArrow => MyConfig.LeaderboardLostArrowConfig.Value;
	private static string LostColor => MyConfig.LeaderboardLostColorConfig.Value;
	private static string EqualSymbol => MyConfig.LeaderboardEqualSymbolConfig.Value;
	private static string EqualColor => MyConfig.LeaderboardEqualColorConfig.Value;

	public override void Enter()
	{
		_teamA = Showdown.Match.TeamA;
		_teamB = Showdown.Match.TeamB;

		foreach (ZeepkistNetworkPlayer zeepkistNetworkPlayer in ZeepkistNetwork.PlayerList.Where(a =>
			         !_teamA.Racers.Exists(r => r.SteamId == a.SteamID) &&
			         !_teamB.Racers.Exists(r => r.SteamId == a.SteamID)))
			ZeepkistNetwork.CustomLeaderBoard_BlockPlayerFromSettingTime(zeepkistNetworkPlayer.SteamID, true);

		Showdown.Match.AddRound(new Round(_teamA, _teamB)); // Initialize round
		_currentRound = Showdown.Match.CurrentRound;

		_teamLeaderboard = new TeamLeaderboard(_currentRound);
		SendTeamLeaderboard();

		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder().ClearChat().TextLine("Race started, glhf").Build().Message);
	}

	public override void Exit()
	{
		foreach (Coroutine coroutine in _resetCoroutines.Values)
			Showdown.StopCoroutine(coroutine);
		_resetCoroutines.Clear();
	}

	public override IState GetNextState()
	{
		return new StatePostRacing(StateMachine);
	}

	public override void OnPlayerJoined(ZeepkistNetworkPlayer player)
	{
		IEnumerable<Racer> allRacers = _currentRound.TeamA.Racers.Concat(_currentRound.TeamB.Racers);
		Racer matchingRacer = allRacers.FirstOrDefault(r => r.SteamId == player.SteamID);

		if (matchingRacer == null)
		{
			return;
		}

		double personalBest = _currentRound.GetPersonalBest(matchingRacer);
		if (!(personalBest < double.MaxValue))
		{
			return;
		}

		ZeepkistNetwork.CustomLeaderBoard_SetPlayerTimeOnLeaderboard(player.SteamID, (float)personalBest, true);
		Team joinedTeam = Showdown.Match.GetTeamBySteamId(player.SteamID);
		string joinedName = GetJsonNameForPlayer(player.SteamID, joinedTeam);
		ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(player.SteamID, "",
			$"<nobr><{joinedTeam.Color}>{joinedName}</color></nobr>", null, null, null);
	}

	public override void OnRoundEnded()
	{
		InvokeFinish();
	}

	public override void OnPlayerResultsChanged(ZeepkistNetworkPlayer player)
	{
		if (player?.CurrentResult == null)
		{
			ChatApi.AddLocalMessage("Error: Received an invalid racer or result.");
			return;
		}

		if (!_teamA.Racers.Exists(r => r.SteamId == player.SteamID)
		    && !_teamB.Racers.Exists(r => r.SteamId == player.SteamID))
		{
			return;
		}

		Racer racer = new(player.SteamID, player.Username);
		Result result = new(racer, player.CurrentResult.Time);

		if (_currentRound == null)
		{
			ChatApi.AddLocalMessage("Error: _currentRound is not initialized.");
			return;
		}

		ulong steamId = player.SteamID;
		Team playerTeam = Showdown.Match.GetTeamBySteamId(steamId);
		string color = playerTeam.Color;
		string name = GetJsonNameForPlayer(steamId, playerTeam);

		List<LeaderboardItem> ingameLeaderboard = ZeepkistNetwork.Leaderboard
			.OrderBy(leaderboard => leaderboard.Time)
			.ToList();

		int? oldPosition = _playerPositions.TryGetValue(steamId, out int existingPosition)
			? existingPosition
			: null;
		double? oldTime = _playerTimes.TryGetValue(steamId, out double existingTime) ? existingTime : null;
		int? newPosition = FindLeaderboardPosition(ingameLeaderboard, steamId);

		if (newPosition.HasValue)
		{
			// Anyone who got overtaken by this new result loses one (or more) positions.
			foreach (KeyValuePair<ulong, int> tracked in _playerPositions.ToList())
			{
				ulong otherId = tracked.Key;
				if (otherId == steamId)
				{
					continue;
				}

				int otherOldPosition = tracked.Value;
				int? otherNewPosition = FindLeaderboardPosition(ingameLeaderboard, otherId);
				if (!otherNewPosition.HasValue || otherNewPosition.Value <= otherOldPosition)
				{
					continue;
				}

				int lost = otherNewPosition.Value - otherOldPosition;
				if (_playerInfo.TryGetValue(otherId, out (string Color, string Name) otherInfo) &&
				    _playerTimes.TryGetValue(otherId, out double otherTime))
				{
					int totalLost = _lostAccum.GetValueOrDefault(otherId, 0) + lost;
					string posOverride = BuildLostPosOverride(totalLost);
					string timeOverride = BuildNormalTimeOverride(otherTime);
					string nameOverride = BuildNameOverride(otherInfo.Color, otherInfo.Name);
					SetLeaderboardOverrides(otherId, timeOverride, nameOverride, posOverride);
					ScheduleLeaderboardReset(otherId, otherTime, otherInfo.Color, otherInfo.Name);
					_lostAccum[otherId] = totalLost;
				}

				_playerPositions[otherId] = otherNewPosition.Value;
			}
		}

		if (newPosition.HasValue && oldPosition.HasValue && newPosition.Value < oldPosition.Value)
		{
			double diff = (oldTime ?? result.Time) - result.Time;
			int gained = oldPosition.Value - newPosition.Value;
			string posOverride = BuildGainedPosOverride(gained);
			string timeOverride = BuildImprovementTimeOverride(diff);
			string nameOverride = BuildNameOverride(color, name);
			SetLeaderboardOverrides(steamId, timeOverride, nameOverride, posOverride);
			ScheduleLeaderboardReset(steamId, result.Time, color, name);
			_lostAccum[steamId] = 0;
		}
		else if (newPosition.HasValue && oldPosition.HasValue && newPosition.Value == oldPosition.Value &&
		         oldTime.HasValue && result.Time < oldTime.Value)
		{
			double diff = oldTime.Value - result.Time;
			string posOverride = BuildSamePosOverride();
			string timeOverride = BuildImprovementTimeOverride(diff);
			string nameOverride = BuildNameOverride(color, name);
			SetLeaderboardOverrides(steamId, timeOverride, nameOverride, posOverride);
			ScheduleLeaderboardReset(steamId, result.Time, color, name);
			_lostAccum[steamId] = 0;
		}
		else if (newPosition.HasValue && !oldPosition.HasValue)
		{
			string posOverride = "<#00ffff>NEW!</color>";
			string timeOverride = $"<#00ffff>{result.Time.GetFormattedTime()}</color>";
			string nameOverride = BuildNameOverride(color, name);
			SetLeaderboardOverrides(steamId, timeOverride, nameOverride, posOverride);
			ScheduleLeaderboardReset(steamId, result.Time, color, name);
			_lostAccum[steamId] = 0;
		}

		if (newPosition.HasValue)
		{
			_playerPositions[steamId] = newPosition.Value;
		}

		_playerTimes[steamId] = result.Time;
		_playerInfo[steamId] = (color, name);

		_currentRound.AddResult(result);
		SendTeamLeaderboard();
	}

	public void SendTeamLeaderboard()
	{
		ServerMessage leaderboardMessage = _teamLeaderboard.GenerateLeaderboardMessage(Showdown.Match);
		leaderboardMessage.Send();
	}

	private static int? FindLeaderboardPosition(List<LeaderboardItem> leaderboard, ulong steamId)
	{
		for (int index = 0; index < leaderboard.Count; index++)
			if (leaderboard[index].SteamID == steamId)
			{
				return index + 1;
			}

		return null;
	}

	private static string BuildNameOverride(string color, string name)
	{
		return $"<nobr><{color}>{name}</color></nobr>";
	}

	private string GetJsonNameForPlayer(ulong steamId, Team team)
	{
		Racer expected = team.ExpectedRacers.FirstOrDefault(r => r.SteamId == steamId);
		string jsonName = expected?.SteamName;
		if (string.IsNullOrEmpty(jsonName))
		{
			return team.GetTag() + " " + (ZeepkistNetwork.PlayerList
				.FirstOrDefault(p => p.SteamID == steamId)?.Username ?? steamId.ToString());
		}

		return team.GetTag() + " " + jsonName;
	}

	private static string BuildNormalTimeOverride(double time)
	{
		return $"<#00000000>-</color>{time.GetFormattedTime()}";
	}

	private static string BuildImprovementTimeOverride(double diff)
	{
		return $"<{GainedColor}>-{TimeSpan.FromSeconds(diff):mm\\:ss\\.fff}</color>";
	}

	private static string BuildGainedPosOverride(int gained)
	{
		return $"<{GainedColor}>{GainedArrow}{gained}</color>";
	}

	private static string BuildLostPosOverride(int lost)
	{
		return $"<{LostColor}>{LostArrow}{lost}</color>";
	}

	private static string BuildSamePosOverride()
	{
		return $"<b><color={EqualColor}>{EqualSymbol}0</color></b>";
	}

	private static void SetLeaderboardOverrides(ulong steamId, string time, string name, string pos = null,
		string s1 = null, string s2 = null)
	{
		ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(steamId, time, name, pos, s1, s2);
	}

	private void SetNormalLeaderboardOverride(ulong steamId, double time, string color, string name)
	{
		string posOverride = _playerPositions.TryGetValue(steamId, out int pos) ? pos.ToString() : null;

		SetLeaderboardOverrides(steamId, BuildNormalTimeOverride(time), BuildNameOverride(color, name), posOverride);
	}

	// Restarts the pending "revert to normal" countdown for this racer, so the very last
	// position/time change is always the one that decides when the override disappears.
	private void ScheduleLeaderboardReset(ulong steamId, double time, string color, string name)
	{
		if (_resetCoroutines.TryGetValue(steamId, out Coroutine existingCoroutine))
		{
			Showdown.StopCoroutine(existingCoroutine);
		}

		_resetCoroutines[steamId] =
			Showdown.StartCoroutine(LeaderboardResetCoroutine(steamId, time, color, name));
	}

	private IEnumerator LeaderboardResetCoroutine(ulong steamId, double time, string color, string name)
	{
		yield return new WaitForSeconds(ResetDelay);

		SetNormalLeaderboardOverride(steamId, time, color, name);
		_lostAccum[steamId] = 0;
		_resetCoroutines.Remove(steamId);
	}
}