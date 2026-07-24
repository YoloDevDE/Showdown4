using System;
using Showdown4.Config;
using Showdown4.Managers;
using Showdown4.States.Showdown;
using Showdown4.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Showdown4.States.Master;

public class StateMasterOn(IStateMachine stateMachine) : IState
{
	private GameObject _showdownObject;

	private ShowdownStateMachine ShowdownStateMachine => SubStateMachine as ShowdownStateMachine;

	public IStateMachine StateMachine { get; } = stateMachine;
	public IStateMachine SubStateMachine { get; set; }
	public event Action Finished;

	public void Enter()
	{
		// ShowdownStateMachine is a MonoBehaviour, so it has to live on a GameObject
		// for Unity to drive its Update() loop. It survives level loads while the
		// showdown is running and is destroyed again in Exit().
		_showdownObject = new GameObject(nameof(ShowdownStateMachine));
		Object.DontDestroyOnLoad(_showdownObject);
		SubStateMachine = _showdownObject.AddComponent<ShowdownStateMachine>();
	}

	public void Exit()
	{
		ChatCommandService.RemoveJoinMessage();
		ChatCommandService.RemoveServerMessage();

		if (!_showdownObject)
		{
			return;
		}

		Object.Destroy(_showdownObject);
		_showdownObject = null;
	}

	public IState GetNextState()
	{
		return new StateMasterOff(StateMachine);
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
		ToastMessenger.LogSuccess($"Season {MyConfig.SeasonNumberConfig.Value} stopped");
	}
}