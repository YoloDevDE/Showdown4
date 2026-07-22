using ZeepSDK.Chat;

namespace Showdown4.Managers;

/// <summary>
///     Central place for every in-game chat command (messages starting with '/').
///     Wraps <see cref="ChatApi" /> so the raw command strings live in exactly one spot
///     instead of being scattered across the states.
/// </summary>
public static class ChatCommandService
{
	public static void RemoveJoinMessage()
	{
		ChatApi.SendMessage("/joinmessage disable");
	}

	public static void RemoveServerMessage()
	{
		ChatApi.SendMessage("/servermessage remove");
	}

	public static void SetTime(int seconds)
	{
		// '/settime' and '/timeset' are aliases for the same in-game command.
		ChatApi.SendMessage($"/settime {seconds}");
	}

	public static void SkipToLevel(int index)
	{
		ChatApi.SendMessage($"/fs {index}");
	}
}