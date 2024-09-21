using ZeepkistNetworking;

namespace Showdown4.Entities;

public class Level
{
    public Level(OnlineZeeplevel onlineZeeplevel)
    {
        OnlineZeeplevel = onlineZeeplevel;
    }

    public OnlineZeeplevel OnlineZeeplevel { get; }
}