using System;

namespace Showdown4.Commands;

public class CommandStartRandom : BaseLocalCommand
{
	public override string Prefix => "/";
	public override string Command => "sd random";
	public override string Description => "Chooses a random map when the Draft is incomplete";

	protected override void OnCommandInvoked(string arguments)
	{
		CommandInvoked?.Invoke();
	}

	public static event Action CommandInvoked;
}