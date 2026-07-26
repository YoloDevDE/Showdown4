using Showdown4.Commands;
using Showdown4.Config;
using Showdown4.Managers;
using Showdown4.States.Showdown;
using Showdown4.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Showdown4.States.Master;

public class StateMasterOn(IStateMachine stateMachine) : MasterStateBase(stateMachine)
{
	private readonly CommandFinishState _finishStateCommand = new();

	private readonly CommandShowdownPause _pauseCommand = new();

	// Commands that only make sense while a showdown is actually running. They are registered on
	// Enter() and removed again on Exit(), so they cannot be used while the plugin is stopped.
	private readonly CommandPrevState _prevStateCommand = new();
	private readonly CommandShowdownRestart _restartCommand = new();
	private readonly CommandRestartState _restartStateCommandRestart = new();
	private readonly CommandShowdownResume _resumeCommand = new();

	private GameObject _showdownObject;

	private ShowdownStateMachine ShowdownStateMachine => SubStateMachine as ShowdownStateMachine;

	public override void Enter()
	{
		RegisterCommands();

		// ShowdownStateMachine is a MonoBehaviour, so it has to live on a GameObject
		// for Unity to drive its Update() loop. It survives level loads while the
		// showdown is running and is destroyed again in Exit().
		_showdownObject = new GameObject(nameof(ShowdownStateMachine));
		Object.DontDestroyOnLoad(_showdownObject);
		SubStateMachine = _showdownObject.AddComponent<ShowdownStateMachine>();
	}

	public override void Exit()
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

	public override IState GetNextState()
	{
		return new StateMasterOff(StateMachine);
	}


	public override void OnFinishState(string arguments)
	{
		SubStateMachine.CurrentState?.InvokeFinish();
	}

	public override void OnPrev()
	{
		SubStateMachine?.TransitionToPreviousState();
	}

	public override void OnPause()
	{
		ShowdownStateMachine?.Pause();
	}

	public override void OnResume()
	{
		ShowdownStateMachine?.Resume();
	}

	public override void OnRestart()
	{
		// Full showdown restart: turn it off and immediately on again. Invoking Finished moves the
		// master machine to StateMasterOff (GetNextState), then OnShowdownStart on that state brings
		// it right back to a fresh StateMasterOn.
		InvokeFinish();
		StateMachine.CurrentState?.OnShowdownStart();
	}

	public override void OnStateRestart()
	{
		SubStateMachine?.RestartCurrentState();
	}

	public override void OnShowdownStart()
	{
		ToastMessenger.LogWarning("already running");
	}

	public override void OnShowdownStop()
	{
		InvokeFinish();
		ToastMessenger.LogSuccess($"Season {MyConfig.SeasonNumberConfig.Value} stopped");
	}

	private void RegisterCommands()
	{
		CommandRegistry.RegisterLocal(_prevStateCommand);
		CommandRegistry.RegisterLocal(_pauseCommand);
		CommandRegistry.RegisterLocal(_resumeCommand);
		CommandRegistry.RegisterLocal(_restartCommand);
		CommandRegistry.RegisterLocal(_finishStateCommand);
		CommandRegistry.RegisterLocal(_restartStateCommandRestart);
	}

	private void UnregisterCommands()
	{
		CommandRegistry.Unregister(_prevStateCommand);
		CommandRegistry.Unregister(_pauseCommand);
		CommandRegistry.Unregister(_resumeCommand);
		CommandRegistry.Unregister(_restartCommand);
		CommandRegistry.Unregister(_finishStateCommand);
		CommandRegistry.Unregister(_restartStateCommandRestart);
	}
}