using System;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandUnLinkRacer : IMixedChatCommand
{
	public string Prefix => "!";
	public string Command => "unlink";
	public string Description => "WIP";

	public void Handle(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId);
	}

	public void Handle(string arguments)
	{
		ChatApi.SendMessage(Prefix + Command + " " + arguments);
		Handle(ZeepkistNetwork.LocalPlayer.SteamID, arguments);
	}

	public static event Action<ulong> CommandInvoked;
}