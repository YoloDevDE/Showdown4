using System;

namespace Showdown4.Commands;

public class CommandBan : BaseMixedCommand
{
	public override string Prefix => "!";
	public override string Command => "ban";
	public override string Description => "!ban <levelindex>";
	public static event Action<ulong, string> CommandInvoked;

	protected override void OnCommandInvoked(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId, arguments);
	}
}