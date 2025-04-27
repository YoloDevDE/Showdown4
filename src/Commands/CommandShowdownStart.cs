using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandShowdownStart : ILocalChatCommand
{
    public string Prefix => "/";
    public string Command => "sd start";
    public string Description => "Starts the Showdown";

    public void Handle(string arguments)
    {
        CommandInvoked?.Invoke();
    }

    public static event Action CommandInvoked;
}