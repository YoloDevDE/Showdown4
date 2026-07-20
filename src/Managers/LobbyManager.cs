using ZeepSDK.Chat;

namespace Showdown4.Managers;

public class LobbyManager
{
	public static void SkipToLevel(int index)
	{
		ChatApi.SendMessage($"/fs {index}");
	}
}