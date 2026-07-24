using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public abstract class BaseLocalCommand : ILocalChatCommand
{
	public abstract string Prefix { get; }
	public abstract string Command { get; }
	public abstract string Description { get; }

	public void Handle(string arguments)
	{
		OnCommandInvoked(arguments);
	}

	protected abstract void OnCommandInvoked(string arguments);
}

public class SomeCommand : ILocalChatCommand
{
	public string Prefix => "/";
	public string Command => "somecommand";
	public string Description => "Some command description";

	public void Handle(string arguments)
	{
		CommandInvoked?.Invoke(arguments);
	}

	public static event Action<string> CommandInvoked;
}