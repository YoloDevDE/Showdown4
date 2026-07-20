using UnityEngine;
using ZeepSDK.Messaging;

namespace Showdown4.Utils;

public static class ToastMessenger
{
	private static readonly ITaggedMessenger TaggedMessenger = MessengerApi.CreateTaggedMessenger("Showdown");

	public static string Tag { get; } = "Showdown";

	public static void Log(string message, float duration = 2.5f)
	{
		TaggedMessenger.Log(message, duration);
	}

	public static void LogSuccess(string message, float duration = 2.5f)
	{
		TaggedMessenger.LogSuccess(message, duration);
	}

	public static void LogWarning(string message, float duration = 2.5f)
	{
		TaggedMessenger.LogWarning(message, duration);
	}

	public static void LogError(string message, float duration = 2.5f)
	{
		TaggedMessenger.LogError(message, duration);
	}

	public static void Log(string message, Color backgroundColor, Color textColor, float duration = 2.5f)
	{
		TaggedMessenger.LogCustomColors(message, textColor, backgroundColor, duration);
	}
}