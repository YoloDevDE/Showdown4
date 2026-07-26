using System;
using ZeepkistClient;

namespace Showdown4.States.Master;

public abstract class MasterStateBase(IStateMachine stateMachine) : IState
{
	public IStateMachine StateMachine { get; } = stateMachine;
	public virtual IStateMachine SubStateMachine { get; protected set; }
	public IState PreviousState { get; set; }

	public event Action Finished;

	public virtual void Enter()
	{
	}

	public virtual void Exit()
	{
	}

	public abstract IState GetNextState();

	public virtual void HandleInput()
	{
	}

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