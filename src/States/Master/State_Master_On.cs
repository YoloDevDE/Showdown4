using System;
using Showdown4.Managers;
using Showdown4.States.Showdown;
using Showdown4.Utils;

namespace Showdown4.States.Master;

public class StateMasterOn(IStateMachine stateMachine) : IState
{
	private ShowdownStateMachine ShowdownStateMachine => SubStateMachine as ShowdownStateMachine;

	public IStateMachine StateMachine { get; } = stateMachine;
	public IStateMachine SubStateMachine { get; set; }
	public event Action Finished;

	public void Enter()
	{
		SubStateMachine = new ShowdownStateMachine();
	}

	public void Exit()
	{
		ChatCommandService.RemoveJoinMessage();
		ChatCommandService.RemoveServerMessage();
	}

	public void InvokeFinish()
	{
		Finished?.Invoke();
	}

	public void OnFinishState(string arguments)
	{
		SubStateMachine.CurrentState?.InvokeFinish();
	}

	public void OnShowdownStart()
	{
		ToastMessenger.LogWarning("already running");
	}

	public void OnShowdownStop()
	{
		Finished?.Invoke();
		ToastMessenger.LogSuccess($"Season {MyConfig.Validated.SeasonNumber} stopped");
	}
}