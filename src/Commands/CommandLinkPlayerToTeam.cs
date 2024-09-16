using System;
using ZeepkistClient;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandLinkPlayerToTeam : IMixedChatCommand
{
    public string Prefix => "!";
    public string Command => "link";
    public string Description => "WIP";

    public void Handle(ulong playerId, string arguments)
    {
        CommandInvoked?.Invoke(playerId);
    }

    public void Handle(string arguments)
    {
        Handle(ZeepkistNetwork.LocalPlayer.SteamID, arguments);
    }

    public static event Action<ulong> CommandInvoked;
}