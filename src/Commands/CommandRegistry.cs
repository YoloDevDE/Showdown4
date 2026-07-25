using ZeepSDK.ChatCommands;

namespace Showdown4.Commands;

// Thin convenience wrapper around ChatCommandApi that (un)registers a command together with the
// aliases it declares (see BaseLocalCommand.Aliases / BaseMixedCommand.Aliases). States use this to
// register their commands in Enter() and remove them again in Exit(), so a command is only known to
// the chat while the state that handles it is actually active.
//
// Note: unregistering a command via ChatCommandApi automatically removes its aliases as well, so the
// Unregister methods only need to unregister the primary command.
public static class CommandRegistry
{
	public static void RegisterLocal<T>(T command) where T : BaseLocalCommand, new()
	{
		ChatCommandApi.RegisterLocalChatCommand(command);
		RegisterAliases(command, command.Aliases);
	}

	public static void RegisterMixed<T>(T command) where T : BaseMixedCommand, new()
	{
		ChatCommandApi.RegisterMixedChatCommand(command);
		RegisterAliases(command, command.Aliases);
	}

	public static void Unregister(BaseLocalCommand command)
	{
		ChatCommandApi.UnregisterLocalChatCommand(command);
	}

	public static void Unregister(BaseMixedCommand command)
	{
		ChatCommandApi.UnregisterMixedChatCommand(command);
	}

	private static void RegisterAliases(ILocalChatCommand command, string[] aliases)
	{
		if (aliases is { Length: > 0 })
		{
			ChatCommandApi.RegisterLocalChatCommandAliases(command, aliases);
		}
	}
}