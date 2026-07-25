using System;

namespace Showdown4.Commands;

public class CommandPick : BaseMixedCommand
{
	public override string Prefix => "!";
	public override string Command => "pick";
	public override string Description => "!pick <levelindex>";
	public override string[] Aliases => new[] { "p" };
	public static event Action<ulong, string> CommandInvoked;

	protected override void OnCommandInvoked(ulong playerId, string arguments)
	{
		CommandInvoked?.Invoke(playerId, arguments);
	}
}