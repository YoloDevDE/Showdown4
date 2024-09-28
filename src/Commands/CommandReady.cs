using System;
using ZeepkistClient;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandReady : IMixedChatCommand
{
    public string Prefix => "!";
    public string Command => "ready";
    public string Description => "Ready Check";

    public void Handle(ulong playerId, string arguments)
    {
        CommandInvoked?.Invoke(playerId, arguments);
    }

    public void Handle(string arguments)
    {
        Handle(ZeepkistNetwork.LocalPlayer.SteamID, arguments);
    }

    public static event Action<ulong, string> CommandInvoked;
}