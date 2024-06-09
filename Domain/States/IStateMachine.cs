namespace Showdown4.Domain.States;

public interface IStateMachine
{
    public IState State { get; set; }
    public void TransitionTo(IState newState);
    public void StopStateMachine();
    public void StartStatMachine(IStateMachine context, IState initialState);
}