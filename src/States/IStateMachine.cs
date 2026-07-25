using System;
using JetBrains.Annotations;
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

	// Concrete state machines that host coroutines (e.g. ShowdownStateMachine) override this to
	// stop every coroutine still running from the previous state. Machines without coroutines
	// simply keep the no-op default.
	void StopAllCoroutines()
	{
	}

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
				StopAllCoroutines();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			CurrentState.Exit();

			// Remember where we came from, so a state always knows its predecessor
			// (used by TransitionToPreviousState / the 'sd prev' command).
			nextState.PreviousState = CurrentState;
		}

		CurrentState = nextState;
		CurrentState.Finished += OnCurrentStateFinished;
		CurrentState.Enter();
		CastBroadcast.OnStateEntered(this); // broadcast match state to companion mods
		CurrentState.SubStateMachine?.Start();
	}

	// Jumps back to the predecessor recorded on the current state (the 'sd prev' command).
	// Does nothing when there is no recorded previous state.
	void TransitionToPreviousState()
	{
		IState previous = CurrentState?.PreviousState;
		if (previous == null)
		{
			Debug.LogWarning("No previous state to transition to.");
			return;
		}

		Debug.Log($"Transitioning back to previous state: {previous.GetType().Name}");
		TransitionTo(previous);
	}

	// Re-enters the current state from scratch (the 'sd restart' command). The recorded
	// PreviousState is preserved so a following 'sd prev' still works as expected.
	void RestartCurrentState()
	{
		IState current = CurrentState;
		if (current == null)
		{
			Debug.LogWarning("No current state to restart.");
			return;
		}

		IState previous = current.PreviousState;
		Debug.Log($"Restarting current state: {current.GetType().Name}");
		TransitionTo(current);
		current.PreviousState = previous;
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