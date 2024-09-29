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

    public OnlineZeeplevel Level { get; }
    public Team Team { get; }
    public bool IsPick { get; }

    public int round { get; set; }

    public string GetActionType()
    {
        return IsPick ? "picked" : "banned";
    }
}