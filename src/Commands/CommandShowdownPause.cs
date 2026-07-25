using System;

namespace Showdown4.Commands;

public class CommandShowdownPause : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd pause";
	public override string Description => "Pauses all timers";
	public static event Action CommandInvoked;

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke();
	}
}