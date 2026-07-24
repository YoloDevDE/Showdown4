namespace Showdown4.Utils;

/// <summary>
///     Central place for all TMP color codes used across the Showdown states and messages.
///     Keeping them here avoids the same hex literals being scattered throughout the code base.
/// </summary>
public static class ShowdownColors
{
	public const string Yellow = "#ffff00";
	public const string Green = "#00ff00";
	public const string Red = "#ff0000";
	public const string Cyan = "#00ffff";
	public const string Gold = "#b19d63";
	public const string White = "#FFFFFF";
	public const string Orange = "#FFA500";
	public const string Gray = "#888888";
	public const string WinnerGold = "#FFD700";
	public const string Purple = "#a855f7";
	public const string Pink = "#ff69b4";
	public const string Blue = "#3399ff";

	// Faint background used to highlight chat commands (e.g. '!link') without being distracting.
	public const string CommandMark = "#00ffff33";

	// Wraps a chat command reference (e.g. "!pick 1-7") in the standard command styling
	// (cyan, italic, faint highlight) for use in plain interpolated strings, mirroring
	// BlockBuilder.Command() used with the ServerMessage builder.
	public static string Command(string text)
	{
		return $"<{Cyan}><i><mark={CommandMark}>{text}</mark></i></color>";
	}
}