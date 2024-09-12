using ZeepkistNetworking;

namespace Showdown4.Tmp;

public class Level
{
    public Level(OnlineZeeplevel onlineZeeplevel)
    {
        OnlineZeeplevel = onlineZeeplevel;
    }

    public OnlineZeeplevel OnlineZeeplevel { get; }
}