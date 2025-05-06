using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandShowdownStop : ILocalChatCommand
{
    public string Prefix => "/";
    public string Command => "sd stop";
    public string Description => "Stops the Showdown";

    public void Handle(string arguments)
    {
        CommandInvoked?.Invoke();
    }

    public static event Action CommandInvoked;
}