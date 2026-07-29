using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

public class StateWarmUp(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// From this many seconds left the countdown is coloured red.
	private const int LowTimeThresholdSeconds = 5;

	private static int WarmUpSeconds => MyConfig.WarmUpSecondsConfig.Value;

	public override void Enter()
	{
		// Block everyone from setting a time while the warm up countdown is running: the warm up is
		// only there to learn the map, nothing of it may ever end up on the leaderboard. It runs
		// exactly once per map, right before the first racing sub-round.
		RacerResetService.BlockEveryoneFromSettingTime();

		ChatMessage.SendCustomMessage(
			new ChatMessage.Builder().ClearChat()
				.DashedLine().NewLine()
				.TextLine($"<b>{Match.UpcomingRoundName()}</b> started")
				.NewLine()
				.DashedLine().NewLine()
				.TextLine($"{Match.Score()}").NewLine()
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
				.AddBlock("<#f00>!!WARMUP!!!</color> Syncing Everyone - Race begins in",
					block => block.Size(20).Color(ShowdownColors.Gray))
				.AddBlock($"{secondsRemaining}s", block => block.Size(20)
					.Color(secondsRemaining <= LowTimeThresholdSeconds
						? ShowdownColors.Red
						: ShowdownColors.Green)));
		introMessage.Send();
	}

	private void OnCountdownComplete()
	{
		ChatMessage.ClearChat();

		// Wipe whatever the warm up produced and respawn everyone, so the first sub-round starts for
		// all racers at the very same moment on a completely empty leaderboard.
		RacerResetService.ClearLeaderboardForEveryone();
		RacerResetService.RespawnEveryone();

		// Times count from here on - the racing state keeps everyone who is not racing blocked.
		RacerResetService.UnblockEveryoneFromSettingTime();
		InvokeFinish();
	}

	private ServerMessage BuildRaceIntroMessage()
	{
		ServerMessage introMessage = new ServerMessage("center")
			.ShowdownHeader(false, "center")
			.AddLine(line => line
				.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
				.AddBlock("VS")
				.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color)));

		// Determine the number of rows needed based on the maximum number of racers in either team
		int maxRacers = Mathf.Max(TeamA.Racers.Count, TeamB.Racers.Count);

		for (int i = 0; i < maxRacers; i++)
		{
			Racer teamARacer = i < TeamA.Racers.Count ? TeamA.Racers[i] : null;
			Racer teamBRacer = i < TeamB.Racers.Count ? TeamB.Racers[i] : null;

			string teamAName = teamARacer?.SteamName ?? " "; // Empty space for alignment
			string teamBName = teamBRacer?.SteamName ?? string.Empty;

			// The teams can have a different amount of racers, so team A may have no racer on this
			// line at all - in that case the single space above is what has to be padded against.
			int padding = Mathf.Max(0,
				1 + TeamA.GetNameWithTag().Length + "VS".Length + TeamB.GetNameWithTag().Length -
				teamAName.Length);

			introMessage.AddLine(line =>
			{
				line.AddBlock(teamAName, f => f.Color(TeamA.Color));

				if (teamBRacer != null)
				{
					line.AddBlock(teamBName.PadLeft(padding), f => f.Color(TeamB.Color));
				}
			});
		}

		introMessage.AddSeparator(TeamA.GetNameWithTag().Length + 4 + TeamB.GetNameWithTag().Length);
		return introMessage;
	}
}