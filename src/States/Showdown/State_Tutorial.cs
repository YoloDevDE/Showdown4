using System;
using System.Collections;
using Showdown4.Config;
using Showdown4.Entities;
using Showdown4.Managers;
using Showdown4.Utils;
using UnityEngine;

namespace Showdown4.States.Showdown;

/// <summary>
///     Idle state that runs right after both teams are linked and before initiative is selected.
///     While players wait, it explains step by step how the whole match is going to play out
///     (match format, round rules, draft rules, tie rules), so nobody is surprised later on.
///     Purely informational - it advances automatically once every step has been shown.
/// </summary>
public class StateTutorial(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		CoroutineManager.Instance.StartExternalCoroutine(TutorialSequence());
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

		yield return ShowStep("Match Format",
			line => line.AddBlock("Best of 3", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("- first team to 2 points wins the match."),
			line => line.AddBlock("Each round lasts"), line =>
				line.AddBlock("5 minutes", block => block.Color(ShowdownColors.Yellow).Bold()));

		yield return ShowStep("Round Rules",
			line => line.AddBlock("Both teams set their best time on the map."),
			line => line.AddBlock("The team with the better"), line =>
				line.AddBlock("average time", block => block.Color(ShowdownColors.Yellow).Bold())
					.AddBlock("wins the round and scores 1 point."));

		yield return ShowStep("Draft Phase",
			line => line.AddBlock("Teams take turns to"), line =>
				line.AddBlock("ban", block => block.Color(ShowdownColors.Red).Bold())
					.AddBlock("or")
					.AddBlock("pick", block => block.Color(ShowdownColors.Green).Bold())
					.AddBlock("maps in an ABAB pattern."),
			line => line.AddBlock("Every team has"), line =>
				line.AddBlock("2 bans", block => block.Color(ShowdownColors.Red))
					.AddBlock("and")
					.AddBlock("1 pick", block => block.Color(ShowdownColors.Green))
					.AddBlock("for the entire match."));

		yield return ShowStep("Ties & Intermission",
			line => line.AddBlock("If the score is"), line =>
				line.AddBlock("1:1", block => block.Color(ShowdownColors.Yellow).Bold())
					.AddBlock("after round 2, a second draft decides the tiebreaker map."));

		ChatMessage.SendCustomMessage(new ChatMessage.Builder().ClearChat().Build().Message);
		InvokeFinish();
	}

	// Shows a single tutorial topic for a configurable duration before moving on to the next one.
	private IEnumerator ShowStep(string title, params Action<ServerMessage.LineBuilder>[] lines)
	{
		ServerMessage msg = new ServerMessage()
			.ShowdownHeader()
			.AddLine(line => line
				.AddBlock("How it all works", builder => builder
					.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold).Bold().AllCaps()
					.Size(30)))
			.AddSeparator()
			.AddLine(line => line.AddBlock(title, block => block.Color(ShowdownColors.Gold).Bold().Size(30)))
			.AddSeparator();

		foreach (Action<ServerMessage.LineBuilder> line in lines) msg.AddLine(line);

		msg.Send();

		yield return new WaitForSeconds(MyConfig.TutorialStepDurationConfig.Value);
	}
}