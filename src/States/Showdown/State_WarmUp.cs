using System.Collections.Generic;
using System.Linq;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Utils;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class StateWarmUp(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private Team _teamA, _teamB;
	private static int WarmUpSeconds => MyConfig.WarmUpSecondsConfig.Value;

	public override void Enter()
	{
		_teamA = Showdown.Match.TeamA;
		_teamB = Showdown.Match.TeamB;

		// Block everyone from setting a time while the warm up countdown is running.
		ZeepkistNetwork.CustomLeaderBoard_BlockEveryoneFromSettingTime(true);

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
				.TextLine($"<i><#c0c0c0>This message disappears in {WarmUpSeconds} seconds</color></i>").Build()
				.Message
		);

		Countdown.Start(WarmUpSeconds, UpdateCountdownMessage, OnCountdownComplete);
	}

	public override IState GetNextState()
	{
		return new StateRacing(StateMachine);
	}

	private void UpdateCountdownMessage(int secondsRemaining)
	{
		if (secondsRemaining <= 0)
		{
			return;
		}

		ServerMessage introMessage = BuildRaceIntroMessage();
		introMessage.AddSeparator()
			.AddLine(line => line
				.AddBlock("Race begins in", block => block.Size(20).Color(ShowdownColors.Gray))
				.AddBlock($"{secondsRemaining}s", block => block.Size(20)
					.Color(secondsRemaining <= 5 ? ShowdownColors.Red : ShowdownColors.Green)));
		introMessage.Send();
	}

	private void OnCountdownComplete()
	{
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);

		// Reset every racer's leaderboard entry so everyone starts the race at the same time.
		List<ulong> steamIds = _teamA.Racers.Concat(_teamB.Racers).Select(r => r.SteamId).Distinct().ToList();

		ZeepkistNetwork.CustomLeaderBoard_ResetPlayers(steamIds);

		ZeepkistNetwork.CustomLeaderBoard_UnblockEveryoneFromSettingTime(true);
		ChatApi.SendMessage("/resettime");
		InvokeFinish();
	}

	private ServerMessage BuildRaceIntroMessage()
	{
		ServerMessage introMessage = new ServerMessage("center")
			.ShowdownHeader(false, "center")
			.AddLine(line => line
				.AddBlock($"{_teamA.GetNameWithTag()}", b => b.Color(_teamA.Color))
				.AddBlock("VS")
				.AddBlock($"{_teamB.GetNameWithTag()}", b => b.Color(_teamB.Color)));

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
		return introMessage;
	}
}