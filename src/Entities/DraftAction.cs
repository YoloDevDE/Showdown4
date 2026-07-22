using ZeepkistNetworking;

namespace Showdown4.Entities;

public class DraftAction(OnlineZeeplevel level, Team team, bool isPick)
{
	public OnlineZeeplevel Level { get; } = level;
	public Team Team { get; } = team;
	public bool IsPick { get; } = isPick;

	public int Round { get; set; }

	public string GetActionType()
	{
		return IsPick ? "picked" : "banned";
	}
}