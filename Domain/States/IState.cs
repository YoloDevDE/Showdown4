using System;

namespace Showdown4.Domain.States;

public interface IState
{
    void Enter(IStateMachine context); // Wird aufgerufen, wenn der Zustand aktiviert wird
    void Exit(); // Wird aufgerufen, wenn der Zustand verlassen wird
    void CheckTransitions();
    IState GetNextState();

    event Action OnCompleted; // Ereignis zur Signalisierung der Fertigstellung
}