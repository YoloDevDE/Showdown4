using System;
using ZeepkistClient;

namespace Showdown4.States;

/// <summary>
///     Shared base class for every state, no matter which state machine it belongs to. It holds the
///     boilerplate that is identical for all of them: the owning state machine, the predecessor used
///     by the 'sd prev' command, the <see cref="Finished" /> event and an empty default
///     implementation for every event hook of <see cref="IState" />.
///     Concrete states (or the machine specific bases <c>MasterStateBase</c>/<c>ShowdownStateBase</c>)
///     only override the few members they actually care about.
/// </summary>
public abstract class StateBase(IStateMachine stateMachine) : IState
{
	public IStateMachine StateMachine { get; } = stateMachine;

	// Only states that own a nested machine (currently just StateMasterOn) assign this.
	public IStateMachine SubStateMachine { get; protected set; }

	public IState PreviousState { get; set; }

	public event Action Finished;

	public virtual void Enter()
	{
	}

	public virtual void Exit()
	{
	}

	public abstract IState GetNextState();

	// States that need keyboard input (e.g. team/initiative selection) override this.
	public virtual void HandleInput()
	{
	}

	// Event hooks delegated from the owning state machine. Declared virtual here so that concrete
	// states can override only the ones they need. The machine subscribes to the underlying events
	// once and forwards them to the current state.
	public virtual void OnRoundStarted()
	{
	}

	public virtual void OnRoundEnded()
	{
	}

	public virtual void OnLevelLoaded()
	{
	}

	public virtual void OnPlayerResultsChanged(ZeepkistNetworkPlayer player)
	{
	}

	public virtual void OnPlayerJoined(ZeepkistNetworkPlayer player)
	{
	}

	public virtual void OnShowdownStart()
	{
	}

	public virtual void OnShowdownStop()
	{
	}

	public virtual void OnFinishState(string arguments)
	{
	}

	public virtual void OnPrev()
	{
	}

	public virtual void OnPause()
	{
	}

	public virtual void OnResume()
	{
	}

	public virtual void OnRestart()
	{
	}

	public virtual void OnStateRestart()
	{
	}

	public virtual void OnBan(ulong steamId, string levelIndex)
	{
	}

	public virtual void OnPick(ulong steamId, string levelIndex)
	{
	}

	public virtual void OnLinkRacer(ulong steamId, string arguments)
	{
	}

	public virtual void OnUnlinkRacer(ulong steamId)
	{
	}

	public virtual void OnPass(ulong steamId)
	{
	}

	public virtual void OnReady(ulong steamId, string arguments)
	{
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}
}