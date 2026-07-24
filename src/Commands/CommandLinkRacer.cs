using System;

namespace Showdown4.Commands;

public class CommandLinkRacer : BaseMixedCommand
{
	public override string Prefix => "!";
	public override string Command => "link";

	public override string Description =>
		"!link <number|TAG> - joins the given team, e.g. '!link 1', '!link #2' or '!link TAG'. Use '!unlink' to leave your team again.";

	protected override void OnCommandInvoked(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId, arguments);
	}

	public static event Action<ulong, string> CommandInvoked;
}