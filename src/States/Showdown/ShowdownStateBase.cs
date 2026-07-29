using Showdown4.Entities;
using Showdown4.Utils;

namespace Showdown4.States.Showdown;

/// <summary>
///     Base class for every state that runs inside the <see cref="ShowdownStateMachine" />.
///     The generic plumbing (state machine, event hooks, <c>Finished</c>) comes from
///     <see cref="StateBase" />; on top of that this class exposes the strongly typed
///     <see cref="Showdown" /> machine together with its <see cref="Match" />/
///     <see cref="CurrentDraft" /> and gives each state its own <see cref="Countdown" />.
/// </summary>
public abstract class ShowdownStateBase(IStateMachine stateMachine) : StateBase(stateMachine)
{
	protected ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;
	protected Match Match => Showdown.Match;
	protected Draft CurrentDraft => Match.CurrentDraft;
	protected Team TeamA => Match.TeamA;
	protected Team TeamB => Match.TeamB;

	// Every state owns its own countdown. It is plain data (a deadline plus callbacks), not a
	// coroutine, so nothing can ever cancel it from the outside. It only advances while this
	// state is the active one (the machine calls Tick() each frame), so a leftover countdown
	// from a previous state can never fire again.
	protected Countdown Countdown { get; } = new();

	// Called once per frame by the ShowdownStateMachine while this state is active.
	public virtual void Tick()
	{
		Countdown.Tick();
	}

	// Freeze / continue this state's own countdown (driven by the 'sd pause'/'sd resume'
	// commands via the ShowdownStateMachine).
	public void PauseCountdown()
	{
		Countdown.Pause();
	}

	public void ResumeCountdown()
	{
		Countdown.Resume();
	}
}