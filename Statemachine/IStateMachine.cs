namespace Showdown4.Statemachine;

public interface IStateMachine
{
    public IState State { get; set; }
    public void TransitionTo(IStateMachine context, IState newState);
    public void StopStateMachine();
    public void StartStatMachine(IStateMachine context, IState initialState);
}