using System.Linq;
using Showdown4.Entities;

namespace Showdown4.Utils;

/// <summary>
///     Reusable <see cref="ServerMessage" /> building blocks that are shared between several states,
///     so the same layout does not have to be copy-pasted (e.g. the "Picked Maps" list used by the
///     ready check and the pre-racing countdown).
/// </summary>
public static class ShowdownMessages
{
	/// <summary>
	///     Appends the "Picked Maps:" header followed by one line per picked level across all drafts.
	///     Rounds that have already been played are struck through.
	/// </summary>
	public static ServerMessage AppendPickedMaps(this ServerMessage msg, Match match)
	{
		msg.AddLine(line => line.AddBlock("Picked Maps:"));

		int round = 1;
		foreach (DraftAction pickedLevel in match.Drafts.SelectMany(draft => draft.PickedLevels))
		{
			int currentRound = round;
			msg.AddLine(line =>
			{
				if (currentRound <= match.RoundCounter())
				{
					line.StrikeThrough();
				}

				line
					.AddBlock($"Round {currentRound}:")
					.AddBlock($"'{pickedLevel.Level.Name}'", block => block.Color(ShowdownColors.Cyan))
					.AddBlock("picked by", block => block.Indent("585%"))
					.AddBlock($"{pickedLevel.Team.GetColoredTag()}")
					.Bold();
			});
			round++;
		}

		return msg;
	}
}