using System;
using ZeepkistClient;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandSkipAction : IMixedChatCommand
{
	public string Prefix => "!";
	public string Command => "skip";
	public string Description => "!skip";

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