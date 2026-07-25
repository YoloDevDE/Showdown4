using System;

namespace Showdown4.Commands;

public class CommandFinishState : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd next";
	public override string Description => "WIP";
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