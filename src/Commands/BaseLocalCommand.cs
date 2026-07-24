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