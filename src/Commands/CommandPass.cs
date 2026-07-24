using System;

namespace Showdown4.Commands;

public class CommandPass : BaseMixedCommand
{
	public override string Prefix => "!";
	public override string Command => "pass";
	public override string Description => "!pass - hands your current draft action over to the other team";
	public static event Action<ulong> CommandInvoked;

	protected override void OnCommandInvoked(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId);
	}
}