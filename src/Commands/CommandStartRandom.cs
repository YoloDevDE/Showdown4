using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public class CommandStartRandom : ILocalChatCommand
{
	public string Prefix => "/";
	public string Command => "sd random";
	public string Description => "Chooses a random map when the Draft is incomplete";

	public void Handle(string arguments)
	{
		CommandInvoked?.Invoke();
	}

	public static event Action CommandInvoked;
}