using ZeepSDK.Chat;

namespace Showdown4.Utils;

public class MyLobbyManager
{
    public static void SkipLevel()
    {
        ChatApi.SendMessage("/fs");
    }


    public static void SetServerMessage(ServerMessageColor serverMessageColor, string message)
    {
        ChatApi.SendMessage($"/servermessage {serverMessageColor.ToString()} 0 {message}");
    }

    public static void SkipToLevel(int index)
    {
        ChatApi.SendMessage($"/fs {index}");
    }
}