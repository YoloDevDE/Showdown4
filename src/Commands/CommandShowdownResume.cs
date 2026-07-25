using System;

namespace Showdown4.Commands;

public class CommandShowdownResume : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd resume";
	public override string Description => "Resumes all paused timers";
	public static event Action CommandInvoked;

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke();
	}
}