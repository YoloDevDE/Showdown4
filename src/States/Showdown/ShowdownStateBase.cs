using System;
using Showdown4.Entities;

namespace Showdown4.States.Showdown;

/// <summary>
///     Base class for every state that runs inside the <see cref="ShowdownStateMachine" />.
///     It captures the boilerplate that used to be copy-pasted into each state:
///     storing the state machine, exposing the strongly typed <see cref="Showdown" /> machine
///     together with its <see cref="Match" />/<see cref="CurrentDraft" />, and raising the
///     <see cref="Finished" /> event through <see cref="InvokeFinish" />.
/// </summary>
public abstract class ShowdownStateBase : IState
{
	protected ShowdownStateBase(IStateMachine stateMachine)
	{
		StateMachine = stateMachine;
	}

	protected ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;
	protected Match Match => Showdown.Match;
	protected Draft CurrentDraft => Match.CurrentDraft;

	public IStateMachine StateMachine { get; }

	public event Action Finished;

	public abstract void Enter();
	public abstract void Execute();
	public abstract void Exit();

	// States that need keyboard input (e.g. team/initiative selection) override this.
	public virtual void HandleInput()
	{
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}
}