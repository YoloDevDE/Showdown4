using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandStartMatch : ILocalChatCommand
{
    public string Prefix => "#";
    public string Command => "start match";
    public string Description => "WIP";

    public void Handle(string arguments)
    {
        CommandInvoked?.Invoke();
    }

    public static event Action CommandInvoked;
}