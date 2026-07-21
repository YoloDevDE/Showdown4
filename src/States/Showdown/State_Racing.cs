using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepkistNetworking;
using ZeepSDK.Chat;
using ZeepSDK.Multiplayer;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class StateRacing : ShowdownStateBase
{
	private readonly Dictionary<ulong, int> _previousPositions = new();
	private readonly Dictionary<ulong, double> _previousTimes = new();
	private Round _currentRound;
	private LeaderboardDisplay _leaderboardDisplay;
	private Team _teamA, _teamB;

	public StateRacing(IStateMachine stateMachine) : base(stateMachine)
	{
	}

	public override void Enter()
	{
		_teamA = Showdown.Match.TeamA;
		_teamB = Showdown.Match.TeamB;
		Showdown.Match.AddRound(new Round(_teamA, _teamB)); // Initialize round
		_currentRound = Showdown.Match.CurrentRound;

		ZeepkistNetwork.PlayerResultsChanged += OnLeaderBoardUpdated;
		RacingApi.RoundEnded += OnRoundEnd;
		MultiplayerApi.PlayerJoined += OnPLayerJoined;
		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder().ClearChat()
				.DashedLine().NewLine()
				.TextLine(
					$"<b>{(Showdown.Match.RoundCounter() == 3 ? "Tiebreaker" : $"Round {Showdown.Match.RoundCounter()}")}</b> started")
				.NewLine()
				.DashedLine().NewLine()
				.TextLine($"{Showdown.Match.Score()}").NewLine()
				.DashedLine().NewLine()
				.TextLine("Good Luck, Have Fun! :smile:").NewLine()
				.TextLine("<i><color=#c0c0c0>This message disappears in 15 seconds</color></i>").Build().Message
		);
		// Start the cool race intro message for the first 10 seconds
		CoroutineManager.Instance.StartExternalCoroutine(DisplayRaceIntroMessage());
	}

	public override void Execute()
	{
		_leaderboardDisplay = new LeaderboardDisplay(_currentRound);
	}

	public override void Exit()
	{
		ZeepkistNetwork.PlayerResultsChanged -= OnLeaderBoardUpdated;

		MultiplayerApi.PlayerJoined -= OnPLayerJoined;
		RacingApi.RoundEnded -= OnRoundEnd;
	}

	private void OnPLayerJoined(ZeepkistNetworkPlayer player)
	{
		IEnumerable<Racer> allRacers = _currentRound.TeamA.Racers.Concat(_currentRound.TeamB.Racers);
		Racer matchingRacer = allRacers.FirstOrDefault(r => r.SteamId == player.SteamID);

		if (matchingRacer != null)
		{
			double personalBest = _currentRound.GetPersonalBest(matchingRacer);
			if (!(personalBest < double.MaxValue))
			{
				return;
			}

			ZeepkistNetwork.CustomLeaderBoard_SetPlayerTimeOnLeaderboard(player.SteamID, (float)personalBest, true);
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(player.SteamID, "",
				$"<nobr><color={Showdown.Match.GetTeamBySteamId(player.SteamID).Color}>" + player.GetTaggedUsername() +
				"</color></nobr>", null, null, null);
		}
	}

	private void OnRoundEnd()
	{
		InvokeFinish();
	}

	private void OnLeaderBoardUpdated(ZeepkistNetworkPlayer player)
	{
		if (player?.CurrentResult == null)
		{
			ChatApi.AddLocalMessage("Error: Received an invalid racer or result.");
			return;
		}

		Racer racer = new(player.SteamID, player.Username);
		Result result = new(racer, player.CurrentResult.Time);

		if (_currentRound == null)
		{
			ChatApi.AddLocalMessage("Error: _currentRound is not initialized.");
			return;
		}


		ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(
			player.SteamID,
			$"<#00ff00>{result.Time.GetFormattedTime()}</color>",
			$"<nobr><color={Showdown.Match.GetTeamBySteamId(player.SteamID).Color}>" + player.GetTaggedUsername() +
			"</color></nobr>",
			null,
			null,
			null
		);
		CoroutineManager.Instance.AddExternalCoroutine(ShowTimeDifferencesTemporarily(player, (float)result.Time));

		_currentRound.AddResult(result);
		SendTeamLeaderboard();
	}

	private IEnumerator ShowTimeDifferencesTemporarily(ZeepkistNetworkPlayer player, float time)
	{
		List<LeaderboardItem> ingameLeaderboard = ZeepkistNetwork.Leaderboard
			.OrderBy(leaderboard => leaderboard.Time)
			.ToList();

		Dictionary<ulong, string> positionChanges = new();
		for (int index = 0; index < ingameLeaderboard.Count; index++)
		{
			int position = index + 1;
			LeaderboardItem currentLeaderboardItem = ingameLeaderboard[index];

			string positionChange = null;
			if (_previousPositions.TryGetValue(currentLeaderboardItem.SteamID, out int previousPosition))
			{
				int change = previousPosition - position;
				if (change != 0)
				{
					string color = change > 0 ? "#11ff03" : "#a52019";
					string sign = change > 0 ? "+" : "";
					positionChange = $"<color={color}>{sign}{change}</color>";
				}
			}

			positionChanges[currentLeaderboardItem.SteamID] = positionChange;

			_previousPositions[currentLeaderboardItem.SteamID] = position;
			string overrideTimeText =
				ZeepkistNetwork.GetLeaderboardOverride(currentLeaderboardItem.SteamID).overrideTimeText;
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(
				currentLeaderboardItem.SteamID,
				overrideTimeText,
				$"<nobr><color={Showdown.Match.GetTeamBySteamId(currentLeaderboardItem.SteamID).Color}>" +
				currentLeaderboardItem.Username +
				"</color></nobr>",
				positionChange,
				null,
				null
			);
		}

		// Track player's previous result time
		double previousTime = _previousTimes.GetValueOrDefault(player.SteamID, 0);

		double timeDiff = time - previousTime;
		string formattedDiff = $"-{TimeSpan.FromSeconds(timeDiff):mm\\:ss\\.fff}";
		string timeColor = "#11ff03";


		// Set the time difference for current player
		ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(
			player.SteamID,
			$"<color={timeColor}>{formattedDiff}</color>",
			$"<nobr><color={Showdown.Match.GetTeamBySteamId(player.SteamID).Color}>" + player.GetTaggedUsername() +
			"</color></nobr>",
			positionChanges[player.SteamID] == null ? "<color=#f7dcaa>=0</color>" : positionChanges[player.SteamID],
			null,
			null
		);

		_previousTimes[player.SteamID] = time;

		yield return new WaitForSeconds(5f);

		// Reset all team players' overrides
		foreach (LeaderboardItem currentLeaderboardItem in ingameLeaderboard)
			ZeepkistNetwork.CustomLeaderBoard_SetPlayerLeaderboardOverrides(
				currentLeaderboardItem.SteamID,
				null,
				$"<nobr><color={Showdown.Match.GetTeamBySteamId(currentLeaderboardItem.SteamID).Color}>" +
				currentLeaderboardItem.Username +
				"</color></nobr>",
				null,
				null,
				null
			);
	}

	public void SendTeamLeaderboard()
	{
		ServerMessage leaderboardMessage = _leaderboardDisplay.GenerateLeaderboardMessage(Showdown.Match);
		leaderboardMessage.Send();
	}

	// Coroutine for showing the cool intro message for the first 10 seconds
// Coroutine for showing the cool intro message for the first 10 seconds
// Coroutine for showing the cool intro message for the first 10 seconds
	private IEnumerator DisplayRaceIntroMessage()
	{
		ServerMessage introMessage = new ServerMessage("center")
				.ShowdownHeader(false, "center")
				.AddLine(line => line
					.AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
					.AddBlock("VS")
					.AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color))
				)
			;

		// Determine the number of rows needed based on the maximum number of racers in either team
		int maxRacers = Mathf.Max(_teamA.Racers.Count, _teamB.Racers.Count);

		for (int i = 0; i < maxRacers; i++)
		{
			Racer teamARacer = i < _teamA.Racers.Count ? _teamA.Racers[i] : null;
			Racer teamBRacer = i < _teamB.Racers.Count ? _teamB.Racers[i] : null;

			introMessage.AddLine(line =>
			{
				if (teamARacer != null)
				{
					line.AddBlock($"{teamARacer.SteamName}", f => f.Color(_teamA.Color));
				}
				else
				{
					line.AddBlock(" "); // Empty space for alignment
				}


				if (teamBRacer != null)
				{
					line.AddBlock(
						$"{teamBRacer.SteamName}".PadLeft(1 + _teamA.GetNameWithTag().Length + "VS".Length +
							_teamB.GetNameWithTag().Length - teamARacer.SteamName.Length), f => f.Color(_teamB.Color));
				}
			});
		}

		introMessage.AddSeparator(_teamA.GetNameWithTag().Length + 4 + _teamB.GetNameWithTag().Length);
		introMessage.Send();

		yield return new WaitForSeconds(15);

		// After 10 seconds, clear the intro message and proceed to the normal race flow
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
		SendTeamLeaderboard();
	}
}