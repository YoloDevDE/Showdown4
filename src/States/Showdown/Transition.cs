using System;

namespace Showdown4.States.Showdown;

public class Transition(IState from, IState to, Func<bool> condition = null) : ITransition
{
	private readonly Func<bool> _condition = condition ?? (() => true); // Condition function

	public IState From { get; } = from;
	public IState To { get; } = to;


	public bool CanTransition()
	{
		return _condition();
		// Evaluate the condition
	}

	public void OnTransition()
	{
		// Optional: Add any transition-specific logic here
	}
}