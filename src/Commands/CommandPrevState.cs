using System;

namespace Showdown4.Commands;

public class CommandPrevState : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd prev";
	public override string Description => "Transitions back to the previous state";
	public static event Action<string> CommandInvoked;

	public static void Fire(string arguments = null)
	{
		CommandInvoked?.Invoke(arguments);
	}

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke(arguments);
	}
}