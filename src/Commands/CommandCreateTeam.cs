using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandCreateTeam : ILocalChatCommand
{
	public string Prefix => "/";
	public string Command => "sd create team";
	public string Description => "WIP";

	public void Handle(string arguments)
	{
		CommandInvoked?.Invoke(arguments);
	}

	public static event Action<string> CommandInvoked;
}