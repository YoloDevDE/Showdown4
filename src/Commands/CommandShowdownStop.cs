using System;

namespace Showdown4.Commands;

public class CommandShowdownStop : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd stop";
	public override string Description => "Stops the Showdown";
	public static event Action CommandInvoked;

	public static void Fire()
	{
		CommandInvoked?.Invoke();
	}

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke();
	}
}