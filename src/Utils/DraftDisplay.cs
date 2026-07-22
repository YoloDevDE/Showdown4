using System.Linq;
using Showdown4.Entities;
using ZeepkistNetworking;

namespace Showdown4.Utils;

/// <summary>
///     Shared rendering helpers for the draft related states. Both <c>StateDrafting</c> and
///     <c>StateDraftCompleted</c> (as well as the incomplete flow) show the same map list and
///     "draft complete" banner, so the building blocks live here to avoid duplication.
/// </summary>
public static class DraftDisplay
{
	public static ServerMessage CompleteHeader(Match match)
	{
		ServerMessage tmp = new();

		tmp.AddLine(line => line
			.AddBlock(match.DraftphaseName,
				builder => builder.Gradients(ShowdownColors.Gold, ShowdownColors.White, ShowdownColors.Gold))
			.AddBlock("-")
			.AddBlock("complete!", builder => builder.Color(ShowdownColors.Green))
			.Bold().AllCaps().Size(40));

		return tmp;
	}

	public static ServerMessage LevelList(Draft draft)
	{
		ServerMessage tmp = new();

		foreach (OnlineZeeplevel level in draft.AllLevels)
			tmp.AddLine(line =>
			{
				line.AddBlock(level.Name, block =>
				{
					if (draft.AvailableLevels.All(l => l.Name != level.Name) ||
					    draft.UnAvailableLevels.Any(l => l.Name == level.Name))
					{
						block.Strikethrough();
					}

					if (draft.UnAvailableLevels.Any(l => l.Name == level.Name))
					{
						block.Color(ShowdownColors.Yellow);
					}

					if (draft.BannedLevels.Any(l => l.Level.Name == level.Name))
					{
						block.Color(ShowdownColors.Red);
					}

					if (draft.PickedLevels.Any(l => l.Level.Name == level.Name))
					{
						block.Color(ShowdownColors.Green);
					}
				});

				DraftAction bannedLevel = draft.BannedLevels.FirstOrDefault(l => l.Level.Name == level.Name);
				DraftAction pickedLevel = draft.PickedLevels.FirstOrDefault(l => l.Level.Name == level.Name);

				if (bannedLevel != null)
				{
					line.AddBlock("banned", block => block.Color(ShowdownColors.Red).Indent("500%"))
						.AddBlock("by")
						.AddBlock($"{bannedLevel.Team.GetTag()}", block => block.Color(bannedLevel.Team.Color));
				}
				else if (pickedLevel != null)
				{
					line.AddBlock("picked", block => block.Color(ShowdownColors.Green).Indent("500%"))
						.AddBlock("by")
						.AddBlock($"{pickedLevel.Team.GetTag()}", block => block.Color(pickedLevel.Team.Color));
				}
			});

		return tmp;
	}
}