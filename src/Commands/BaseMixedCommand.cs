using ZeepkistClient;
using ZeepSDK.Chat;
using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

public abstract class BaseMixedCommand : IMixedChatCommand
{
	public abstract string Prefix { get; }
	public abstract string Command { get; }
	public abstract string Description { get; }

	public void Handle(ulong playerId, string arguments)
	{
		OnCommandInvoked(playerId, arguments);
	}

	public void Handle(string arguments)
	{
		ChatApi.SendMessage(Prefix + Command + " " + arguments);
		Handle(ZeepkistNetwork.LocalPlayer.SteamID, arguments);
	}

	protected abstract void OnCommandInvoked(ulong playerId, string arguments);
}