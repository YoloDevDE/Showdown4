using System;
using Showdown4.Commands;

namespace Showdown4.States.Master;

public class MasterStateMachine : IStateMachine
{
	public IState CurrentState { get; set; }

	// Instead of assigning a value in the constructor, use properties that return new instances
	public IState InitialState => new StateMasterOff(this);
	public IState FinalState => new StateMasterOff(this);

	public event Action StateMachineFinished;

	public void InvokeFinish()
	{
		StateMachineFinished?.Invoke();
	}

	public void SubscribeToEvents()
	{
		CommandShowdownStart.CommandInvoked += HandleShowdownStart;
		CommandShowdownStop.CommandInvoked += HandleShowdownStop;
		CommandFinishState.CommandInvoked += HandleFinishState;
		CommandShowdownPrev.CommandInvoked += HandlePrev;
		CommandShowdownPause.CommandInvoked += HandlePause;
		CommandShowdownResume.CommandInvoked += HandleResume;
		CommandShowdownRestart.CommandInvoked += HandleRestart;
		CommandStateRestart.CommandInvoked += HandleStateRestart;
	}

	public void UnsubscribeFromEvents()
	{
		CommandShowdownStart.CommandInvoked -= HandleShowdownStart;
		CommandShowdownStop.CommandInvoked -= HandleShowdownStop;
		CommandFinishState.CommandInvoked -= HandleFinishState;
		CommandShowdownPrev.CommandInvoked -= HandlePrev;
		CommandShowdownPause.CommandInvoked -= HandlePause;
		CommandShowdownResume.CommandInvoked -= HandleResume;
		CommandShowdownRestart.CommandInvoked -= HandleRestart;
		CommandStateRestart.CommandInvoked -= HandleStateRestart;
	}

	private void HandleShowdownStart()
	{
		CurrentState?.OnShowdownStart();
	}

	private void HandleShowdownStop()
	{
		CurrentState?.OnShowdownStop();
	}

	private void HandleFinishState(string arguments)
	{
		CurrentState?.OnFinishState(arguments);
	}

	private void HandlePrev(string arguments)
	{
		CurrentState?.OnPrev();
	}

	private void HandlePause()
	{
		CurrentState?.OnPause();
	}

	private void HandleResume()
	{
		CurrentState?.OnResume();
	}

	private void HandleRestart(string arguments)
	{
		CurrentState?.OnRestart();
	}

	private void HandleStateRestart(string arguments)
	{
		CurrentState?.OnStateRestart();
	}
}