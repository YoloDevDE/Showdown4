using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandStopMatch : ILocalChatCommand
{
    public string Prefix => "#";
    public string Command => "stop match";
    public string Description => "WIP";

    public void Handle(string arguments)
    {
        CommandInvoked?.Invoke();
    }

    public static event Action CommandInvoked;
}