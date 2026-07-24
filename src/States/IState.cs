using System;
using ZeepkistClient;

namespace Showdown4.States;

public interface IState
{
	public IStateMachine StateMachine { get; }

	public IStateMachine SubStateMachine => null;

	event Action Finished;
	void Enter();

	// Called once this state is finished, to determine which state comes next.
	// Each state decides its own successor(s) instead of a central transition table.
	IState GetNextState();

	void HandleInput()
	{
	}

	void Exit();
	void InvokeFinish();

	// Event hooks delegated from the owning state machine. States override only the
	// hooks they care about instead of subscribing/unsubscribing to the events themselves.

	// Racing events
	void OnRoundStarted()
	{
	}

	void OnRoundEnded()
	{
	}

	void OnLevelLoaded()
	{
	}

	// Networking events
	void OnPlayerResultsChanged(ZeepkistNetworkPlayer player)
	{
	}

	void OnPlayerJoined(ZeepkistNetworkPlayer player)
	{
	}

	// Master command events
	void OnShowdownStart()
	{
	}

	void OnShowdownStop()
	{
	}

	void OnFinishState(string arguments)
	{
	}

	// Showdown command events
	void OnBan(ulong steamId, string levelIndex)
	{
	}

	void OnPick(ulong steamId, string levelIndex)
	{
	}

	void OnLinkRacer(ulong steamId, string arguments)
	{
	}

	void OnUnlinkRacer(ulong steamId)
	{
	}

	void OnPass(ulong steamId)
	{
	}

	void OnReady(ulong steamId, string arguments)
	{
	}
}