using System.Collections;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

/// <summary>
///     The last state before <see cref="StateDrafting" />. It plays the draft-phase intro animation and
///     then clearly announces which team has initiative. In Draftphase I the rules were already
///     explained by <see cref="StateDraftReadyCheck" />, which runs right before this state; Draftphase
///     II has no ready check, so a short instruction reminder is shown here instead.
/// </summary>
public class StatePreDraft(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// Pause between two steps of the intro animation.
	private const float IntroStepSeconds = 1.5f;

	// From this many seconds left the countdowns are coloured red.
	private const int LowTimeThresholdSeconds = 5;

	private Team InitiativeTeam => Match.Initiative;

	public override void Enter()
	{
		Showdown.StartCoroutine(IntroSequence());
	}

	public override IState GetNextState()
	{
		return new StateDrafting(StateMachine);
	}

	private IEnumerator IntroSequence()
	{
		ChatMessage.ClearChat();
		ChatMessage.SendCustomMessage(
			$"<{ShowdownColors.Orange}><b>*** <{ShowdownColors.Gold}>{Match.UpcomingDraftPhaseName}</color> loading - Please pay attention to the upcoming messages! ***</b></color>");
		ChatCommandService.RemoveServerMessage();
		yield return new WaitForSeconds(IntroStepSeconds);

		ServerMessage initiationMessage = new ServerMessage("center").ShowdownHeader(false, "center", 50);
		initiationMessage.Send();
		yield return new WaitForSeconds(IntroStepSeconds);

		initiationMessage.AddLine(l =>
			l.Size(30).Bold().AddBlock(Match.ScoreWithFullNameColoredAndPadded()).NoBreak());
		initiationMessage.Send();
		yield return new WaitForSeconds(IntroStepSeconds);

		initiationMessage.AddLine(line =>
			line.AddBlock(Match.UpcomingDraftPhaseName,
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold()
					.AllCaps().Size(40)));
		initiationMessage.Send();
		yield return new WaitForSeconds(IntroStepSeconds);

		int initiativeDuration = Mathf.Max(MyConfig.InitiativeAnnouncementDurationConfig.Value, 1);
		for (int remaining = initiativeDuration; remaining > 0; remaining--)
		{
			SendInitiativeMessage(remaining);
			yield return new WaitForSeconds(1f);
		}

		// The first draft phase is preceded by the ready check (StateDraftReadyCheck), which explains
		// the draft rules itself - nothing more needs to happen here. Every later phase has no ready
		// check anymore, so a short instruction reminder is shown here instead.
		if (Match.UpcomingDraftPhaseNumber > 1)
		{
			ShowInstructions();

			int instructionDuration = Mathf.Max(MyConfig.PreDraftInstructionDurationConfig.Value, 1);
			for (int remaining = instructionDuration - 1; remaining > 0; remaining--)
			{
				yield return new WaitForSeconds(1f);
				SendInstructionsServerMessage(remaining);
			}
		}

		InvokeFinish();
	}

	private void ShowInstructions()
	{
		ChatMessage.ClearChat();
		ChatMessage.SendCustomMessage(
			$"<{ShowdownColors.Gold}><b>*** How the draft works ***</b></color>" +
			$"<br><{ShowdownColors.Yellow}>1.</color> When it's your turn you either <{ShowdownColors.Red}>ban</color> or <{ShowdownColors.Green}>pick</color> a map." +
			$"<br><{ShowdownColors.Yellow}>2.</color> Type {ShowdownColors.Command("!ban 1-7")} to remove a map you don't want to play." +
			$"<br><{ShowdownColors.Yellow}>3.</color> Type {ShowdownColors.Command("!pick 1-7")} to choose a map that will be played." +
			$"<br><{ShowdownColors.Yellow}>4.</color> Type {ShowdownColors.Command("!pass")} to hand your current action over to the other team." +
			$"<br><{ShowdownColors.Gray}><i>Every selection is FINAL and cannot be undone!</i></color>");

		SendInstructionsServerMessage(Mathf.Max(MyConfig.PreDraftInstructionDurationConfig.Value, 1));
	}

	private void SendInitiativeMessage(int remaining)
	{
		new ServerMessage("center").ShowdownHeader(false, "center", 50)
			.AddLine(line => line.Size(30).Bold().AddBlock(Match.ScoreWithFullNameColoredAndPadded()).NoBreak())
			.AddLine(line => line
				.AddBlock(Match.UpcomingDraftPhaseName, builder => builder
					.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold().AllCaps()
					.Size(40)))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("INITIATIVE", block => block.Color(ShowdownColors.Gold).Bold().AllCaps().Size(30)))
			.AddLine(line => line
				.AddBlock(InitiativeTeam.GetNameWithTag(), block => block.Color(InitiativeTeam.Color).Bold().Size(40)))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("Draft starts in", block => block.Size(20).Color(ShowdownColors.Gray))
				.AddBlock($"{remaining}s", block => block.Size(20)
					.Color(remaining <= LowTimeThresholdSeconds ? ShowdownColors.Red : ShowdownColors.Green)))
			.Send();
	}

	private void SendInstructionsServerMessage(int remaining)
	{
		new ServerMessage().ShowdownHeader(true)
			.AddLine(line => line
				.AddBlock("How to draft", builder => builder
					.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold().AllCaps()
					.Size(40)))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock("!ban 1-7", block => block.Command())
				.AddBlock("- remove a map"))
			.AddLine(line => line
				.AddBlock("!pick 1-7", block => block.Command())
				.AddBlock("- choose a map to play"))
			.AddLine(line => line
				.AddBlock("!pass", block => block.Command())
				.AddBlock("- give your action to the other team"))
			.AddSeparator()
			.AddLine(line => line
				.Italic()
				.AddBlock("Draft starts in", block => block.Color(ShowdownColors.Gray))
				.AddBlock($"{remaining}s",
					block => block.Color(remaining <= LowTimeThresholdSeconds
						? ShowdownColors.Red
						: ShowdownColors.Green)))
			.Send();
	}
}