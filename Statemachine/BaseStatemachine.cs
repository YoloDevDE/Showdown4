namespace Showdown4.Statemachine;

public abstract class BaseStatemachine : IStateMachine
{
    public abstract IState State { get; set; }


    public abstract void StopStateMachine();

    public void StartStatMachine(IStateMachine context, IState initialState)
    {
        context.State = initialState;
        context.State.Enter(context);
    }

    public void TransitionTo(IStateMachine context, IState newState)
    {
        context.State?.Exit();
        context.State = newState;
        context.State.Enter(context);
    }
}