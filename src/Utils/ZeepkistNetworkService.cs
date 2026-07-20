using System.Linq;
using ZeepkistClient;

namespace Showdown4.Utils;

public class ZeepkistNetworkService
{
	public static string GetSteamNameFromSteamId(ulong steamId)
	{
		return ZeepkistNetwork.PlayerList.FirstOrDefault(player => player.SteamID == steamId)?.GetUserNameNoTag();
	}
}