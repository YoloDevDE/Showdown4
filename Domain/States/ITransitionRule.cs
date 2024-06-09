namespace Showdown4.Domain.States;

public interface ITransitionRule
{
    bool ShouldTransition(); // Bedingung für den Übergang
    IState GetNextState(); // Der nächste Zustand bei erfüllter Bedingung
}