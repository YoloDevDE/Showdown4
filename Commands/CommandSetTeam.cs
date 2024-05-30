using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandSetTeam : ILocalChatCommand
{
    public string Prefix => "#";
    public string Command => "set teams";
    public string Description => "WIP";

    public void Handle(string arguments)
    {
        CommandInvoked?.Invoke(arguments);
    }

    public static event Action<string> CommandInvoked;
}