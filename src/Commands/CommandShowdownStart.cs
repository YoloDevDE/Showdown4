using System;

namespace Showdown4.Commands;

public class CommandShowdownStart : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd start";
	public override string Description => "Starts the Showdown";

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke();
	}

	public static event Action CommandInvoked;
}