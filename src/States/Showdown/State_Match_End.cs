using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

public class StateMatchEnd(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private bool _countdownStarted;
	private int _countdownTime = MyConfig.MatchEndKickCountdownConfig.Value; // Countdown duration in seconds

	public override void Enter()
	{
		ChatCommandService.SetTime(86400);
		if (!_countdownStarted)
		{
			_countdownStarted = true;

			// Use CountdownTimer to manage the countdown
			CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
				_countdownTime,
				UpdateCountdownMessage, // onTick action
				InvokeFinish // onComplete action
			));
		}
	}

	private void SendServerMessage()
	{
		// We don't need to send this message every second manually now, since it's handled in `UpdateCountdownMessage`
		Team winnerTeam = Match.CurrentRound.GetWinnerTeam;

		// Decorated ServerMessage without emotes
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Match Over!", block => block.Bold().Color(ShowdownColors.Red))
			)
			.AddLine(line => line
				.AddBlock($"{winnerTeam.GetFullNameWithTag()} ", block => block.Color(winnerTeam.Color).Bold())
				.AddBlock("won the Match! :party:", block => block.Color(ShowdownColors.WinnerGold))
			)
			.AddLine(line => line
					.AddBlock("Final Score: ", block => block.Bold().Color(ShowdownColors.White))
					.AddBlock(Match.ScoreColored()) // Green for the final score
			)
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("You can now join us in Discord for an interview :smile:")
			)
			.AddLine(line => line
				.AddBlock("Teams will get kicked after")
				.AddBlock($"{_countdownTime}",
					block => block.Bold().Color(_countdownTime <= 10 ? ShowdownColors.Red : ShowdownColors.Green))
				.AddBlock("seconds.")
			)
			.AddSeparator();

		msg.Send();
	}

	public override void Exit()
	{
		// No extra cleanup required here
	}

	private void UpdateCountdownMessage(int remainingTime)
	{
		_countdownTime = remainingTime;
		SendServerMessage(); // This will send the updated message with the countdown
	}
}