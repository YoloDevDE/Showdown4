using ZeepkistNetworking;

namespace Showdown4.Entities;

public class DraftAction
{
	public DraftAction(OnlineZeeplevel level, Team team, bool isPick)
	{
		Level = level;
		Team = team;
		IsPick = isPick;
	}

	private DraftAction(Team team)
	{
		Team = team;
		IsPass = true;
	}

	public OnlineZeeplevel Level { get; }
	public Team Team { get; }
	public bool IsPick { get; }
	public bool IsPass { get; }

	// Creates a "pass" action that carries no level - the team simply handed its turn over.
	public static DraftAction CreatePass(Team team)
	{
		return new DraftAction(team);
	}

	public string GetActionType()
	{
		if (IsPass)
		{
			return "passed";
		}

		return IsPick ? "picked" : "banned";
	}
}