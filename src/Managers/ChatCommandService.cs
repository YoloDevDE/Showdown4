using ZeepSDK.Chat;

namespace Showdown4.Managers;

/// <summary>
///     Central place for every in-game chat command (messages starting with '/').
///     Wraps <see cref="ChatApi" /> so the raw command strings live in exactly one spot
///     instead of being scattered across the states.
/// </summary>
public static class ChatCommandService
{
	// One full day in seconds, used for the "+24h" timer trick (see SetRaceTime/SuspendTimer).
	private const int TimerDayOffsetSeconds = 86400;

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

	// Sets the lobby timer for a racing sub-round. A full day is added on top of the wanted
	// duration because the in-game timer only displays hours/minutes/seconds up to 24h: the HUD
	// therefore still shows the intended duration (e.g. 01:02) while the lobby never reaches zero
	// on its own and thus never ends the round / switches the map behind our back.
	public static void SetRaceTime(int seconds)
	{
		SetTime(TimerDayOffsetSeconds + seconds);
	}

	// Parks the lobby timer a full day in the future so it never runs out on its own. Used for every
	// phase in which nothing is being raced (setup, drafting, pauses, after the match), because a
	// timer reaching zero would end the round and switch the map behind our back.
	public static void SuspendTimer()
	{
		SetTime(TimerDayOffsetSeconds);
	}

	public static void SkipToLevel(int index)
	{
		ChatApi.SendMessage($"/fs {index}");
	}

	// Forces the lobby to leave the current level. Needed because the "+24h" timer never runs out on
	// its own, so the map has to be skipped manually once every sub-round of it has been raced.
	public static void ForceSkipLevel()
	{
		ChatApi.SendMessage("/fs");
	}
}