using System.Collections;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

/// <summary>
///     Runs right before <see cref="StateDrafting" /> (Draftphase II) or <see cref="StateDraftReadyCheck" />
///     (Draftphase I). It plays the draft-phase intro animation, then clearly announces which team has
///     initiative. For Draftphase I the ready check (which itself explains pick/ban/pass) takes over
///     right after that; for Draftphase II - which has no ready check - a short instruction reminder is
///     shown here instead.
/// </summary>
public class StatePreDraft(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	private Team InitiativeTeam => Match.Initiative;

	public override void Enter()
	{
		Showdown.StartCoroutine(IntroSequence());
	}

	public override void Exit()
	{
	}

	public override IState GetNextState()
	{
		return new StateDrafting(StateMachine);
	}

	private IEnumerator IntroSequence()
	{
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
		ChatMessage.SendCustomMessage(
			$"<{ShowdownColors.Orange}><b>*** <{ShowdownColors.Gold}>{Match.DraftphaseName}</color> loading - Please pay attention to the upcoming messages! ***</b></color>");
		ChatCommandService.RemoveServerMessage();
		yield return new WaitForSeconds(1.5f);

		ServerMessage initiationMessage = new ServerMessage("center").ShowdownHeader(false, "center", 50);
		initiationMessage.Send();
		yield return new WaitForSeconds(1.5f);

		initiationMessage.AddLine(l =>
			l.Size(30).Bold().AddBlock(Match.ScoreWithFullNameColoredAndPadded()).NoBreak());
		initiationMessage.Send();
		yield return new WaitForSeconds(1.5f);

		initiationMessage.AddLine(line =>
			line.AddBlock(Match.DraftphaseName,
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold()
					.AllCaps().Size(40)));
		initiationMessage.Send();
		yield return new WaitForSeconds(1.5f);

		int initiativeDuration = Mathf.Max(MyConfig.InitiativeAnnouncementDurationConfig.Value, 1);
		for (int remaining = initiativeDuration; remaining > 0; remaining--)
		{
			SendInitiativeMessage(remaining);
			yield return new WaitForSeconds(1f);
		}

		// Draftphase I: the ready check (StateDraftReadyCheck) explains the draft rules itself,
		// so nothing more needs to happen here.
		// Draftphase II: there is no ready check anymore, so a short instruction reminder is shown here.
		if (Match.IsDraftphaseTwo)
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
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
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
				.AddBlock(Match.DraftphaseName, builder => builder
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
					.Color(remaining <= 5 ? ShowdownColors.Red : ShowdownColors.Green)))
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
					block => block.Color(remaining <= 5 ? ShowdownColors.Red : ShowdownColors.Green)))
			.Send();
	}
}