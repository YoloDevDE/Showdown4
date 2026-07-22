using System;
using Showdown4.Utils;

namespace Showdown4.States.Master;

public class StateMasterOff(IStateMachine stateMachine) : IState
{
	public IStateMachine StateMachine { get; } = stateMachine;

	public event Action Finished;

	public void Enter()
	{
	}

	public void Exit()
	{
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}

	public void OnShowdownStop()
	{
		ToastMessenger.LogWarning("already stopped");
	}

	public void OnShowdownStart()
	{
		Finished?.Invoke();

		ToastMessenger.LogSuccess($"Season {MyConfig.Validated.SeasonNumber} started");
	}
}