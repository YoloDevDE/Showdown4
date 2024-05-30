namespace Showdown4.Statemachine;

public interface IState
{
    void Enter(IStateMachine context);
    void Exit();
}