using System;
using JetBrains.Annotations;
using Showdown4.Utils;
using Debug = UnityEngine.Debug;

namespace Showdown4.States;

internal static class StateMachineOperations
{
	public static void Dispose(IStateMachine stateMachine)
	{
		try
		{
			stateMachine.TransitionTo(stateMachine.FinalState);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

		stateMachine.CurrentState.Exit();
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
		if (stateMachine.CurrentState != null)
		{
			stateMachine.CurrentState.SubStateMachine?.Dispose();
			stateMachine.CurrentState.Finished -= stateMachine.OnCurrentStateFinished;
			try
			{
				stateMachine.StopAllCoroutines();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			stateMachine.CurrentState.Exit();
			nextState.PreviousState = stateMachine.CurrentState;
		}

		stateMachine.CurrentState = nextState;
		stateMachine.CurrentState.Finished += stateMachine.OnCurrentStateFinished;
		stateMachine.CurrentState.Enter();
		CastBroadcast.OnStateEntered(stateMachine);
		stateMachine.CurrentState.SubStateMachine?.Start();
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
		IState nextState = stateMachine.CurrentState.GetNextState();
		if (nextState == null)
		{
			Debug.LogError($"No next state defined for the current state: {stateMachine.CurrentState.GetType().Name}");
			return;
		}

		Debug.Log(
			$"Current State: {stateMachine.CurrentState.GetType().Name}, Transitioning to: {nextState.GetType().Name}");
		stateMachine.TransitionTo(nextState);
	}
}