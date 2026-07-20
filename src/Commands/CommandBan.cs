using System;
using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandBan : IMixedChatCommand
{
	public string Prefix => "!";
	public string Command => "ban";
	public string Description => "!ban <levelindex>";

	public void Handle(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId, arguments);
	}

	public void Handle(string arguments)
	{
		ChatApi.SendMessage(Prefix + Command + " " + arguments);
		Handle(ZeepkistNetwork.LocalPlayer.SteamID, arguments);
	}

	public static event Action<ulong, string> CommandInvoked;
}