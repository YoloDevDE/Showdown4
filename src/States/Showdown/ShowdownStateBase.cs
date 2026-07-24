using System;
using Showdown4.Entities;
using ZeepkistClient;

namespace Showdown4.States.Showdown;

/// <summary>
///     Base class for every state that runs inside the <see cref="ShowdownStateMachine" />.
///     It captures the boilerplate that used to be copy-pasted into each state:
///     storing the state machine, exposing the strongly typed <see cref="Showdown" /> machine
///     together with its <see cref="Match" />/<see cref="CurrentDraft" />, and raising the
///     <see cref="Finished" /> event through <see cref="InvokeFinish" />.
/// </summary>
public abstract class ShowdownStateBase(IStateMachine stateMachine) : IState
{
	protected ShowdownStateMachine Showdown => (ShowdownStateMachine)StateMachine;
	protected Match Match => Showdown.Match;
	protected Draft CurrentDraft => Match.CurrentDraft;
	public IStateMachine StateMachine { get; } = stateMachine;

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

	// Event hooks delegated from the ShowdownStateMachine. Declared virtual here so that
	// concrete states can override only the ones they need. The machine subscribes to the
	// underlying events once and forwards them to the current state.
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