namespace Showdown4.States;

public interface ITransition
{
	IState From { get; }
	IState To { get; }

	// Condition to check if transition is valid
	bool CanTransition();

	void OnTransition(); // Optional: Logic to run during transition
}