using BepInEx.Logging;

namespace Showdown4.Utils;

public static class ModLogger
{
	public static ManualLogSource Logger { get; private set; }

	public static void Initialize(ManualLogSource logSource)
	{
		Logger = logSource;
	}

	public static void LogInfo(string message)
	{
		Logger?.LogInfo(message);
	}

	public static void LogError(string message)
	{
		Logger?.LogError(message);
	}

	public static void LogWarning(string message)
	{
		Logger?.LogWarning(message);
	}
}