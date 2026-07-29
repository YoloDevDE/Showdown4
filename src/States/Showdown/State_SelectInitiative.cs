using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

/// <summary>
///     Initiative is determined automatically from the qualification times stored in Teams.json: the
///     team with the better (lower) average qualification time gets it - there is no manual selection.
/// </summary>
public class StateSelectInitiative(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private Team _selectedTeam; // The team with initiative

	public override void Enter()
	{
		// The team with the lower (faster) average qualification time automatically gets initiative.
		_selectedTeam = TeamA.QualificationTime <= TeamB.QualificationTime ? TeamA : TeamB;
		Match.Initiative = _selectedTeam;

		BuildQualificationMessage().Send();

		Countdown.Start(MyConfig.InitiativeAnnouncementDurationConfig.Value, UpdateCountdownMessage, InvokeFinish);
	}

	public override IState GetNextState()
	{
		return new StateDraftReadyCheck(StateMachine);
	}

	// The qualification time comparison both teams see, with or without the initiative result below.
	private ServerMessage BuildQualificationMessage()
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
		BuildQualificationMessage()
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Initiative has been given to")
			)
			.AddLine(line => line
				.AddBlock(_selectedTeam.GetNameWithTag(), f => f.Color(_selectedTeam.Color).Bold()))
			.Send();
	}
}