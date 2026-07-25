using System;

namespace Showdown4.Commands;

public class CommandShowdownPrev : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd prev";
	public override string Description => "Transitions back to the previous state";
	public static event Action<string> CommandInvoked;

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke(arguments);
	}
}