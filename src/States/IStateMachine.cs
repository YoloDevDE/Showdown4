using System;
using JetBrains.Annotations;

namespace Showdown4.States;

public interface IStateMachine
{
	IState CurrentState { get; set; }
	[NotNull] IState InitialState { get; }
	[NotNull] IState FinalState { get; }

	event Action StateMachineFinished;

	void InvokeFinish();

	void StopAllCoroutines();
	void Dispose();
	void StateMachineFinishedNotify();
	void Start();

	void SubscribeToEvents();
	void UnsubscribeFromEvents();
	void TransitionTo([NotNull] IState nextState);

	void TransitionToPreviousState();
	void RestartCurrentState();
	void OnCurrentStateFinished();
}