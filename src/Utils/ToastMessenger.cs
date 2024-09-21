using UnityEngine;
using ZeepSDK.Messaging;

namespace Showdown4.Utils;

public static class ToastMessenger
{
    private static readonly ITaggedMessenger _taggedMessenger = MessengerApi.CreateTaggedMessenger("Showdown");

    public static string Tag { get; } = "Showdown";

    public static void Log(string message, float duration = 2.5f)
    {
        _taggedMessenger.Log(message, duration);
    }

    public static void LogSuccess(string message, float duration = 2.5f)
    {
        _taggedMessenger.LogSuccess(message, duration);
    }

    public static void LogWarning(string message, float duration = 2.5f)
    {
        _taggedMessenger.LogWarning(message, duration);
    }

    public static void LogError(string message, float duration = 2.5f)
    {
        _taggedMessenger.LogError(message, duration);
    }

    public static void Log(string message, Color backgroundColor, Color textColor, float duration = 2.5f)
    {
        _taggedMessenger.LogCustomColors(message, textColor, backgroundColor, duration);
    }
}