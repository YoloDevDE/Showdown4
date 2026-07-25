using System;

namespace Showdown4.Commands;

public class CommandShowdownStart : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd start";
	public override string Description => "Starts the Showdown";
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