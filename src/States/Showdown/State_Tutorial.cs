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

	public override void Exit()
	{
	}

	public override IState GetNextState()
	{
		return new StateSelectInitiative(StateMachine);
	}

	private IEnumerator TutorialSequence()
	{
		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);

		foreach (TutorialContent.Page page in TutorialContent.Pages) yield return ShowStep(page);

		HasPlayedOnce = true;

		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
		InvokeFinish();
	}

	// Shows a single tutorial topic for a configurable duration before moving on to the next one.
	private IEnumerator ShowStep(TutorialContent.Page page)
	{
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock("How it all works", builder => builder
					.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold().AllCaps()
					.Size(30)))
			.AddSeparator()
			.AddLine(line => line.AddBlock(page.Title, block => block.Color(ShowdownColors.Gold).Bold().Size(30)))
			.AddSeparator();

		foreach (Action<ServerMessage.LineBuilder> line in page.Lines) msg.AddLine(line);

		msg.Send();

		yield return new WaitForSeconds(MyConfig.TutorialStepDurationConfig.Value);
	}
}