using System;

namespace Showdown4.Commands;

public class CommandShowdownStop : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd stop";
	public override string Description => "Stops the Showdown";

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke();
	}

	public static event Action CommandInvoked;
}