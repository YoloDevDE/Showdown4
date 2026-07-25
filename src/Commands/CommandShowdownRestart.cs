using System;

namespace Showdown4.Commands;

public class CommandShowdownRestart : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd restart";
	public override string Description => "Restarts the whole showdown (off and on again)";
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