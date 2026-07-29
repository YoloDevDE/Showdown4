using System;
using System.Collections;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

/// <summary>
///     Idle state that runs right after both teams are linked and before initiative is selected.
///     While players wait, it explains step by step how the whole match is going to play out
///     (tournament basics, match flow, win conditions, disconnect handling, then the draft rules),
///     so nobody is surprised later on. Purely informational - it advances automatically once every
///     step has been shown.
///     The very first time this ever runs in a session it plays as its own full-screen sequence.
///     Every time after that, the same content is instead sprinkled into <see cref="StateDraftReadyCheck" />
///     as rotating "loading screen" tips, since everyone has already seen the full explanation once.
/// </summary>
public class StateTutorial(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	// Session-scoped on purpose: the full tutorial only needs to play once per plugin session.
	// Every following match reuses the same content as quick tips during the ready check instead.
	public static bool HasPlayedOnce { get; private set; }

	public override void Enter()
	{
		if (HasPlayedOnce)
		{
			// Already explained once this session - TutorialContent will resurface as rotating
			// tips during the ready check instead of blocking everyone with the full sequence again.
			InvokeFinish();
			return;
		}

		Showdown.StartCoroutine(TutorialSequence());
	}

	public override IState GetNextState()
	{
		return new StateSelectInitiative(StateMachine);
	}

	private IEnumerator TutorialSequence()
	{
		ChatMessage.ClearChat();

		for (int pageIndex = 0; pageIndex < TutorialContent.Pages.Count; pageIndex++)
			yield return ShowStep(TutorialContent.Pages[pageIndex], pageIndex + 1, TutorialContent.Pages.Count);

		HasPlayedOnce = true;

		ChatMessage.ClearChat();
		InvokeFinish();
	}

	// Shows a single tutorial topic for a configurable duration before moving on to the next one.
	private IEnumerator ShowStep(TutorialContent.Page page, int pageNumber, int pageCount)
	{
		int duration = Mathf.Max(MyConfig.TutorialStepDurationConfig.Value, 1);
		for (int remaining = duration; remaining > 0; remaining--)
		{
			SendStepMessage(page, pageNumber, pageCount, remaining);
			yield return new WaitForSeconds(1f);
		}
	}

	private static void SendStepMessage(TutorialContent.Page page, int pageNumber, int pageCount, int remaining)
	{
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock("How it all works", builder => builder
					.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold().AllCaps()
					.Size(30)))
			.AddSeparator()
			.AddLine(line => line
				.AddBlock(page.Title, block => block.Color(ShowdownColors.Gold).Bold().Size(30))
				.AddBlock($"({pageNumber}/{pageCount}) next:{remaining}",
					builder => builder.Color(ShowdownColors.Gray).Size(25)))
			.AddSeparator();

		foreach (Action<ServerMessage.LineBuilder> line in page.Lines) msg.AddLine(line);

		msg.Send();
	}
}