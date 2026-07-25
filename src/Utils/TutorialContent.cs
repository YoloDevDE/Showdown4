using System;
using System.Collections.Generic;

namespace Showdown4.Utils;

/// <summary>
///     Single source of truth for every tutorial topic. Used both by
///     <see cref="Showdown4.States.Showdown.StateTutorial" />
///     (full-screen sequence, shown once per session) and by <see cref="Showdown4.States.Showdown.StateDraftReadyCheck" />
///     (rotating "loading screen" tips, shown every time after that).
///     The pages walk through everything in the order players actually experience it:
///     tournament basics -&gt; how a match flows from draft to the final round -&gt; how a round is won
///     -&gt; what happens if you disconnect -&gt; and finally, once the "big picture" is clear, how the
///     draft itself works in detail.
/// </summary>
public static class TutorialContent
{
	public static readonly List<Page> Pages = new()
	{
		new Page("Tournament Basics",
			line => line.AddBlock("Best of 3", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("- first team to 2 points wins the match."),
			line => line.AddBlock("Each round lasts"), line =>
				line.AddBlock("5 minutes", block => block.Color(ShowdownColors.Yellow).Bold())),

		new Page("Match Flow",
			line => line.AddBlock("A match plays out like this:"),
			line => line
				.AddBlock("Draft 1", block => block.Color(ShowdownColors.Gold).Bold())
				.AddBlock("->")
				.AddBlock("Round 1", block => block.Color(ShowdownColors.Cyan))
				.AddBlock("->")
				.AddBlock("Round 2", block => block.Color(ShowdownColors.Cyan)),
			line => line
				.AddBlock("If the score is")
				.AddBlock("1:1", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("after round 2:")
				.AddBlock("Draft 2", block => block.Color(ShowdownColors.Gold).Bold())
				.AddBlock("->")
				.AddBlock("Round 3", block => block.Color(ShowdownColors.Cyan))
				.AddBlock("->")
				.AddBlock("Match End", block => block.Color(ShowdownColors.Green).Bold()),
			line => line
				.AddBlock("Otherwise the match simply ends right there:")
				.AddBlock("Match End", block => block.Color(ShowdownColors.Green).Bold())),

		new Page("Win Conditions",
			line => line.AddBlock("Both teams set their best time on the map."),
			line => line.AddBlock("The team with the better"), line =>
				line.AddBlock("average time", block => block.Color(ShowdownColors.Yellow).Bold())
					.AddBlock("of both racers wins the round and scores 1 point.")),

		new Page("Disconnects? No Worries!",
			line => line
				.AddBlock("If you disconnect, don't panic -")
				.AddBlock("your result is saved!", block => block.Color(ShowdownColors.Green).Bold()),
			line => line
				.AddBlock("Simply")
				.AddBlock("rejoin", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("and you'll continue right where you left off."),
			line => line
				.AddBlock("Just keep in mind: the clock")
				.AddBlock("keeps running", block => block.Color(ShowdownColors.Red).Bold())
				.AddBlock("while you're gone!")),

		new Page("Draft Basics",
			line => line.AddBlock("Every team has its own draft inventory for the entire match:"),
			line => line
				.AddBlock("2 bans", block => block.Color(ShowdownColors.Red).Bold())
				.AddBlock("and")
				.AddBlock("1 pick", block => block.Color(ShowdownColors.Green).Bold()),
			line => line.AddBlock("Teams take turns to"), line =>
				line.AddBlock("ban", block => block.Color(ShowdownColors.Red).Bold())
					.AddBlock("or")
					.AddBlock("pick", block => block.Color(ShowdownColors.Green).Bold())
					.AddBlock("maps in an ABAB pattern.")),

		new Page("Draft Initiative",
			line => line
				.AddBlock("Draft 1:")
				.AddBlock("the team with the better", block => block.Color(ShowdownColors.Gold))
				.AddBlock("average qualifier time", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("drafts first."),
			line => line
				.AddBlock("Draft 2:")
				.AddBlock("initiative switches to the", block => block.Color(ShowdownColors.Gold))
				.AddBlock("counter-team", block => block.Color(ShowdownColors.Yellow).Bold())
				.AddBlock("- whoever didn't have it in Draft 1.")),

		new Page("Draft 1 vs. Draft 2",
			line => line
				.AddBlock("Maps")
				.AddBlock("banned", block => block.Color(ShowdownColors.Red).Bold())
				.AddBlock("in Draft 1")
				.AddBlock("come back", block => block.Color(ShowdownColors.Green).Bold())
				.AddBlock("for Draft 2."),
			line => line
				.AddBlock("Maps that were already")
				.AddBlock("played", block => block.Color(ShowdownColors.Cyan).Bold())
				.AddBlock("stay out for good.")),

		new Page("Passing Your Turn",
			line => line
				.AddBlock("Use")
				.AddBlock("!pass", block => block.Command())
				.AddBlock("to hand your action over to the other team."),
			line => line
				.AddBlock("This only works if the other team")
				.AddBlock("still has an action left", block => block.Color(ShowdownColors.Yellow))
				.AddBlock("to use it for."))
	};

	public readonly struct Page(string title, params Action<ServerMessage.LineBuilder>[] lines)
	{
		public string Title { get; } = title;
		public Action<ServerMessage.LineBuilder>[] Lines { get; } = lines;
	}
}