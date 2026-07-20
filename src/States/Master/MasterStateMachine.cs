using System;
using System.Collections.Generic;

namespace Showdown4.States.Master;

public class MasterStateMachine : IStateMachine
{
	public MasterStateMachine()
	{
		Transitions = new List<ITransition>();
	}

	public IState CurrentState { get; set; }

	// Instead of assigning a value in the constructor, use properties that return new instances
	public IState InitialState => new StateMasterOff(this);
	public IState FinalState => new StateMasterOff(this);

	public List<ITransition> Transitions { get; set; }
	public event Action StateMachineFinished;

	public void InitTransitions()
	{
		IStateMachine stateMachine = this;
		Transitions = new List<ITransition>();
		// Add transitions with new instances directly
		stateMachine
			.AddTransition(new StateMasterOff(this), new StateMasterOn(this))
			.AddTransition(new StateMasterOn(this), new StateMasterOff(this));
	}

	public void InvokeFinish()
	{
		StateMachineFinished?.Invoke();
	}
}