using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Debug = UnityEngine.Debug;

namespace Showdown4.Domain.States;

public interface IStateMachine
{
    IState CurrentState { get; set; }
    [NotNull] IState InitialState { get; }
    [NotNull] IState FinalState { get; }

    protected List<ITransition> Transitions { get; }

    public IStateMachine AddTransition(ITransition transition)
    {
        Transitions.Add(transition);
        return this;
    }


    event Action StateMachineFinished;

    protected void TransitionTo([NotNull] IState nextState)
    {
        if (CurrentState != null)
        {
            CurrentState.SubStateMachine?.Dispose();
            CurrentState.Finished -= OnCurrentStateFinished;
            CurrentState.Exit();
        }

        CurrentState = nextState;
        CurrentState.Finished += OnCurrentStateFinished;
        CurrentState.Enter();
        CurrentState.Execute();
        CurrentState.SubStateMachine?.Start();
    }

    private void OnCurrentStateFinished()
    {
        foreach (ITransition transition in Transitions)
        {
            if (transition.From != CurrentState || !transition.CanTransition())
            {
                continue;
            }

            // Log the current state and the next state
            Debug.Log($"Current State: {CurrentState}, Transitioning to: {transition.To}");

            // Perform the transition
            transition.OnTransition();
            TransitionTo(transition.To);
            return;
        }

        Debug.LogError($"No valid transition found for the current state: {CurrentState}");
    }


    void Dispose()
    {
        TransitionTo(FinalState);
        CurrentState.Exit();
    }

    void StateMachineFinishedNotify()
    {
        InvokeFinish();
    }


    void Start()
    {
        InitTransitions();
        TransitionTo(InitialState);
    }

    void InitTransitions();
    void InvokeFinish();
}