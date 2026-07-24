using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

// Initiative is now determined automatically from the qualification times stored in Teams.json:
// the team with the better (lower) average qualification time gets initiative - no manual selection.
public class StateSelectInitiative(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private const int CountdownDuration = 2; // Countdown in seconds
	private Team _selectedTeam; // The team with initiative

	private Team TeamA => Match.TeamA;
	private Team TeamB => Match.TeamB;

	public override void Enter()
	{
		// The team with the lower (faster) average qualification time automatically gets initiative.
		_selectedTeam = TeamA.QualificationTime <= TeamB.QualificationTime ? TeamA : TeamB;
		Showdown.Match.Initiative = _selectedTeam;

		ServerMessageThing().Send();

		CoroutineManager.Instance.StartExternalCoroutine(
			CountdownTimer.Start(CountdownDuration, UpdateCountdownMessage, InvokeFinish)
		);
	}

	public override void Exit()
	{
	}

	public override IState GetNextState()
	{
		return new StatePreDraft(StateMachine);
	}

	private ServerMessage ServerMessageThing()
	{
		return new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock($"{TeamA.GetNameWithTag()}", b => b.Color(TeamA.Color))
				.AddBlock("VS")
				.AddBlock($"{TeamB.GetNameWithTag()}", b => b.Color(TeamB.Color))
			)
			.AddSeparator()
			.AddLine(line => line.AddBlock("Determining Initiative from Qualification Times"))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock($"{TeamA.GetTag()}", block => block.Color(TeamA.Color))
				.AddBlock("Avg. Qualification Time:")
				.AddBlock($"{TeamA.QualificationTime:F3}s", block => block.Color(ShowdownColors.Yellow)))
			.AddLine(line => line
				.AddBlock($"{TeamB.GetTag()}", block => block.Color(TeamB.Color))
				.AddBlock("Avg. Qualification Time:")
				.AddBlock($"{TeamB.QualificationTime:F3}s", block => block.Color(ShowdownColors.Yellow)));
	}

	private void UpdateCountdownMessage(int countdownTime)
	{
		// Send or append the countdown message to the server
		ServerMessage msg =
			ServerMessageThing()
				.AddSeparator()
				.AddLine(line => line
					.AddBlock("Initiative has been given to")
				)
				.AddLine(line => line
					.AddBlock(_selectedTeam.GetNameWithTag(), f => f.Color(_selectedTeam.Color).Bold()));
		msg.Send();
	}
}