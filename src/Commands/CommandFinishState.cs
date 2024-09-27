using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandFinishState : ILocalChatCommand
{
    public string Prefix => "/";
    public string Command => "sd next";
    public string Description => "WIP";

    public void Handle(string arguments)
    {
        CommandInvoked?.Invoke(arguments);
    }

    public static event Action<string> CommandInvoked;
}