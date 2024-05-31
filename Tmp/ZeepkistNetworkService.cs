using System.Linq;
using ZeepkistClient;

namespace Showdown4.Tmp;

public class ZeepkistNetworkService
{
    public string GetSteamNameFromSteamId(ulong steamId)
    {
        return ZeepkistNetwork.PlayerList.FirstOrDefault(player => player.SteamID == steamId)?.GetUserNameNoTag();
    }
}