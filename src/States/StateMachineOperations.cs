using System;
using JetBrains.Annotations;
using Showdown4.Utils;
using Debug = UnityEngine.Debug;

namespace Showdown4.States;

/// <summary>
///     The actual transition logic of every state machine. Both <c>MasterStateMachine</c> and
///     <c>ShowdownStateMachine</c> only forward their <see cref="IStateMachine" /> members to the
///     methods here, so the behaviour stays identical for both of them.
/// </summary>
internal static class StateMachineOperations
{
	public static void Dispose(IStateMachine stateMachine)
	{
		// The final state gets the chance to clean the lobby up (unblock racers, stop timers, ...)
		// before it is left again right away.
		try
		{
			stateMachine.TransitionTo(stateMachine.FinalState);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}

		stateMachine.CurrentState?.Exit();
		stateMachine.UnsubscribeFromEvents();
	}

	public static void StateMachineFinishedNotify(IStateMachine stateMachine)
	{
		stateMachine.InvokeFinish();
	}

	public static void Start(IStateMachine stateMachine)
	{
		stateMachine.SubscribeToEvents();
		stateMachine.TransitionTo(stateMachine.InitialState);
	}

	public static void TransitionTo(IStateMachine stateMachine, [NotNull] IState nextState)
	{
		IState previousState = stateMachine.CurrentState;
		if (previousState != null)
		{
			previousState.SubStateMachine?.Dispose();
			previousState.Finished -= stateMachine.OnCurrentStateFinished;
			try
			{
				stateMachine.StopAllCoroutines();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			previousState.Exit();
			nextState.PreviousState = previousState;
		}

		stateMachine.CurrentState = nextState;
		nextState.Finished += stateMachine.OnCurrentStateFinished;
		nextState.Enter();
		CastBroadcast.OnStateEntered(stateMachine);
		nextState.SubStateMachine?.Start();
	}

	public static void TransitionToPreviousState(IStateMachine stateMachine)
	{
		IState previous = stateMachine.CurrentState?.PreviousState;
		if (previous == null)
		{
			Debug.LogWarning("No previous state to transition to.");
			return;
		}

		Debug.Log($"Transitioning back to previous state: {previous.GetType().Name}");
		stateMachine.TransitionTo(previous);
	}

	public static void RestartCurrentState(IStateMachine stateMachine)
	{
		IState current = stateMachine.CurrentState;
		if (current == null)
		{
			Debug.LogWarning("No current state to restart.");
			return;
		}

		IState previous = current.PreviousState;
		Debug.Log($"Restarting current state: {current.GetType().Name}");
		stateMachine.TransitionTo(current);
		current.PreviousState = previous;
	}

	public static void OnCurrentStateFinished(IStateMachine stateMachine)
	{
		IState currentState = stateMachine.CurrentState;
		if (currentState == null)
		{
			Debug.LogWarning("A state finished although the state machine has no current state.");
			return;
		}

		IState nextState = currentState.GetNextState();
		if (nextState == null)
		{
			Debug.LogError($"No next state defined for the current state: {currentState.GetType().Name}");
			return;
		}

		Debug.Log($"Current State: {currentState.GetType().Name}, Transitioning to: {nextState.GetType().Name}");
		stateMachine.TransitionTo(nextState);
	}
}