using System;

namespace Showdown4.Commands;

public class CommandReady : BaseMixedCommand
{
	public override string Prefix => "!";
	public override string Command => "ready";
	public override string Description => "Confirms if you are ready for showdwon!.";
	public static event Action<ulong, string> CommandInvoked;

	protected override void OnCommandInvoked(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId, arguments);
	}
}