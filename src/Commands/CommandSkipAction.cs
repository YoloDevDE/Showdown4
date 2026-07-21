using System;

namespace Showdown4.Commands;

public class CommandSkipAction : BaseMixedCommand
{
	public override string Prefix => "!";
	public override string Command => "skip";
	public override string Description => "!skip";

	protected override void OnCommandInvoked(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId, arguments);
	}

	public static event Action<ulong, string> CommandInvoked;
}