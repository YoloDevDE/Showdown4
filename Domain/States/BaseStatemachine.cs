namespace Showdown4.Domain.States;

public abstract class BaseStateMachine<TState> : IStateMachine where TState : IState
{
    private TState currentState;

    public IState State { get; set; }

    public void TransitionTo(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
            currentState.OnCompleted -= OnStateCompleted;
        }

        currentState = (TState)newState;
        currentState.OnCompleted += OnStateCompleted;
        currentState.Enter(this);
    }


    public abstract void StopStateMachine();

    public void StartStatMachine(IStateMachine context, IState initialState)
    {
        context.State = initialState;
        context.State.Enter(context);
    }

    private void OnStateCompleted()
    {
        IState nextState = currentState.GetNextState();
        TransitionTo(nextState);
    }
}