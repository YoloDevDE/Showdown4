using System;
using Showdown4.Commands;
using Showdown4.Config;
using Showdown4.Managers;
using Showdown4.States.Showdown;
using Showdown4.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Showdown4.States.Master;

public class StateMasterOn(IStateMachine stateMachine) : IState
{
	private readonly CommandFinishState _finishStateCommand = new();

	private readonly CommandShowdownPause _pauseCommand = new();

	// Commands that only make sense while a showdown is actually running. They are registered on
	// Enter() and removed again on Exit(), so they cannot be used while the plugin is stopped.
	private readonly CommandShowdownPrev _prevCommand = new();
	private readonly CommandShowdownRestart _restartCommand = new();
	private readonly CommandShowdownResume _resumeCommand = new();
	private readonly CommandStateRestart _stateRestartCommand = new();

	private GameObject _showdownObject;

	private ShowdownStateMachine ShowdownStateMachine => SubStateMachine as ShowdownStateMachine;

	public IStateMachine StateMachine { get; } = stateMachine;
	public IStateMachine SubStateMachine { get; set; }
	public IState PreviousState { get; set; }
	public event Action Finished;

	public void Enter()
	{
		RegisterCommands();

		// ShowdownStateMachine is a MonoBehaviour, so it has to live on a GameObject
		// for Unity to drive its Update() loop. It survives level loads while the
		// showdown is running and is destroyed again in Exit().
		_showdownObject = new GameObject(nameof(ShowdownStateMachine));
		Object.DontDestroyOnLoad(_showdownObject);
		SubStateMachine = _showdownObject.AddComponent<ShowdownStateMachine>();
	}

	public void Exit()
	{
		UnregisterCommands();

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

	public void OnPrev()
	{
		SubStateMachine?.TransitionToPreviousState();
	}

	public void OnPause()
	{
		ShowdownStateMachine?.Pause();
	}

	public void OnResume()
	{
		ShowdownStateMachine?.Resume();
	}

	public void OnRestart()
	{
		// Full showdown restart: turn it off and immediately on again. Invoking Finished moves the
		// master machine to StateMasterOff (GetNextState), then OnShowdownStart on that state brings
		// it right back to a fresh StateMasterOn.
		Finished?.Invoke();
		StateMachine.CurrentState?.OnShowdownStart();
	}

	public void OnStateRestart()
	{
		SubStateMachine?.RestartCurrentState();
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

	private void RegisterCommands()
	{
		CommandRegistry.RegisterLocal(_prevCommand);
		CommandRegistry.RegisterLocal(_pauseCommand);
		CommandRegistry.RegisterLocal(_resumeCommand);
		CommandRegistry.RegisterLocal(_restartCommand);
		CommandRegistry.RegisterLocal(_finishStateCommand);
		CommandRegistry.RegisterLocal(_stateRestartCommand);
	}

	private void UnregisterCommands()
	{
		CommandRegistry.Unregister(_prevCommand);
		CommandRegistry.Unregister(_pauseCommand);
		CommandRegistry.Unregister(_resumeCommand);
		CommandRegistry.Unregister(_restartCommand);
		CommandRegistry.Unregister(_finishStateCommand);
		CommandRegistry.Unregister(_stateRestartCommand);
	}
}