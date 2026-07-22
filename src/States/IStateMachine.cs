using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Showdown4.Managers;
using Showdown4.States.Showdown;
using Debug = UnityEngine.Debug;

namespace Showdown4.States;

public interface IStateMachine
{
	IState CurrentState { get; set; }
	[NotNull] IState InitialState { get; }
	[NotNull] IState FinalState { get; }

	protected List<ITransition> Transitions { get; }

	// Overload that accepts two states and defaults condition to true
	public IStateMachine AddTransition(IState from, IState to)
	{
		return AddTransition(new Transition(from, to, () => true));
	}

	// Overload that accepts two states with a condition
	public IStateMachine AddTransition(IState from, IState to, Func<bool> condition)
	{
		return AddTransition(new Transition(from, to, condition));
	}

	// Original method that accepts an ITransition object
	public IStateMachine AddTransition(ITransition transition)
	{
		Transitions.Add(transition);
		return this;
	}

	event Action StateMachineFinished;

	protected void TransitionTo([NotNull] IState nextState)
	{
		InitTransitions();
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
		CurrentState.SubStateMachine?.Start();
	}

	private void OnCurrentStateFinished()
	{
		foreach (ITransition transition in Transitions)
		{
			if (transition.From.GetType().Name != CurrentState.GetType().Name || !transition.CanTransition())
			{
				continue;
			}

			Debug.Log(
				$"Current State: {CurrentState.GetType().Name}, Transitioning to: {transition.To.GetType().Name}");

			transition.OnTransition();
			TransitionTo(transition.To);
			return;
		}

		Debug.LogError($"No valid transition found for the current state: {CurrentState.GetType().Name}");
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
		InitTransitions();
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

	void InitTransitions();
	void InvokeFinish();
}