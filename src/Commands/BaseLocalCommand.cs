using System;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public abstract class BaseLocalCommand : ILocalChatCommand
{
	// Additional keywords that trigger this command. They share the same Prefix as the primary
	// command. Empty by default; commands override as needed.
	public virtual string[] Aliases => Array.Empty<string>();
	public abstract string Prefix { get; }
	public abstract string Command { get; }
	public abstract string Description { get; }

	public void Handle(string arguments)
	{
		OnCommandInvoked(arguments);
	}

	protected abstract void OnCommandInvoked(string arguments);
}