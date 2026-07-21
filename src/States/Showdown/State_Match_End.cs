using System;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using ZeepSDK.Chat;

namespace Showdown4.States.Showdown;

public class StateMatchEnd : IState
{
	private bool _countdownStarted;
	private int _countdownTime = 60; // Countdown duration set to 60 seconds

	public StateMatchEnd(IStateMachine stateMachine)
	{
		StateMachine = stateMachine;
	}

	public ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;
	public IStateMachine StateMachine { get; }

	public event Action Finished;

	public void Enter()
	{
		ChatApi.SendMessage("/timeset 86400");
		if (!_countdownStarted)
		{
			_countdownStarted = true;

			// Use CountdownTimer to manage the countdown
			CoroutineManager.Instance.StartExternalCoroutine(CountdownTimer.Start(
				_countdownTime,
				remainingTime => UpdateCountdownMessage(remainingTime), // onTick action
				InvokeFinish // onComplete action
			));
		}
	}

	public void Execute()
	{
		// We don't need to send this message every second manually now, since it's handled in `UpdateCountdownMessage`
		Team winnerTeam = Showdown.Match.CurrentRound.GetWinnerTeam;

		// Decorated ServerMessage without emotes
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Match Over!", block => block.Bold().Color("#ff0000"))
			)
			.AddLine(line => line
				.AddBlock($"{winnerTeam.GetFullNameWithTag()} ", block => block.Color(winnerTeam.Color).Bold())
				.AddBlock("won the Match! :party:", block => block.Color("#FFD700"))
			)
			.AddLine(line => line
					.AddBlock("Final Score: ", block => block.Bold().Color("#ffffff"))
					.AddBlock(Showdown.Match.ScoreColored()) // Green for the final score
			)
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("You can now join us in Discord for an interview :smile:")
			)
			.AddLine(line => line
				.AddBlock("Teams will get kicked after")
				.AddBlock($"{_countdownTime}",
					block => block.Bold().Color(_countdownTime <= 10 ? "#ff0000" : "#00ff00"))
				.AddBlock("seconds.")
			)
			.AddSeparator();

		msg.Send();
	}

	public void Exit()
	{
		// No extra cleanup required here
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}

	private void UpdateCountdownMessage(int remainingTime)
	{
		_countdownTime = remainingTime;
		Execute(); // This will send the updated message with the countdown
	}
}