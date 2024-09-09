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

    protected Dictionary<IState, IState> Transitions { get; }

    public IStateMachine AddTransition([NotNull] IState from, [NotNull] IState to)
    {
        Transitions.Add(from, to);
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
        CurrentState.SubStateMachine?.Init();
    }

    private void OnCurrentStateFinished()
    {
        if (Transitions.ContainsKey(CurrentState))
        {
            // Log the current state and the next state
            Debug.Log($"Current State: {CurrentState}, Transitioning to: {Transitions[CurrentState]}");

            // Transition to the next state
            TransitionTo(Transitions[CurrentState]);
        }
        else
        {
            Debug.LogError($"No transition found for the current state: {CurrentState}");
        }
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

    void Init()
    {
        TransitionTo(InitialState);
    }

    void InvokeFinish();
}