using System;
using JetBrains.Annotations;
using Showdown4.Managers;
using Showdown4.Utils;
using Debug = UnityEngine.Debug;

namespace Showdown4.States;

public interface IStateMachine
{
	IState CurrentState { get; set; }
	[NotNull] IState InitialState { get; }
	[NotNull] IState FinalState { get; }

	event Action StateMachineFinished;

	void InvokeFinish();

	void Dispose()
	{
		try
		{
			TransitionTo(FinalState);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

		CurrentState.Exit();
		UnsubscribeFromEvents();
	}

	void StateMachineFinishedNotify()
	{
		InvokeFinish();
	}

	void Start()
	{
		SubscribeToEvents();
		TransitionTo(InitialState);
	}

	// Central event subscription. Each concrete state machine subscribes to its own events
	// once and delegates them to the current state via the IState event hooks.
	void SubscribeToEvents()
	{
	}

	void UnsubscribeFromEvents()
	{
	}

	protected void TransitionTo([NotNull] IState nextState)
	{
		if (CurrentState != null)
		{
			CurrentState.SubStateMachine?.Dispose();
			CurrentState.Finished -= OnCurrentStateFinished;
			try
			{
				CoroutineManager.Instance.StopAllExternalCoroutines();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			CurrentState.Exit();
		}

		CurrentState = nextState;
		CurrentState.Finished += OnCurrentStateFinished;
		CurrentState.Enter();
		CastBroadcast.OnStateEntered(this); // broadcast match state to companion mods
		CurrentState.SubStateMachine?.Start();
	}

	// Each state decides its own successor via GetNextState() - no central transition table.
	private void OnCurrentStateFinished()
	{
		IState nextState = CurrentState.GetNextState();
		if (nextState == null)
		{
			Debug.LogError($"No next state defined for the current state: {CurrentState.GetType().Name}");
			return;
		}

		Debug.Log($"Current State: {CurrentState.GetType().Name}, Transitioning to: {nextState.GetType().Name}");
		TransitionTo(nextState);
	}
}