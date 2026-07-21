using System;

namespace Showdown4.Commands;

public class CommandUnLinkRacer : BaseMixedCommand
{
	public override string Prefix => "!";
	public override string Command => "unlink";
	public override string Description => "WIP";

	protected override void OnCommandInvoked(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId);
	}

	public static event Action<ulong> CommandInvoked;
}