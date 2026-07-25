using System;

namespace Showdown4.Commands;

public class CommandStateRestart : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd restartstate";
	public override string Description => "Restarts the current state";
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