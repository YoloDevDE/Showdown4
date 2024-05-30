namespace Showdown4.Statemachine;

public class State_Drafting : IState
{
    private MatchStateMachine _context;

    public void Enter(IStateMachine context)
    {
        _context = (MatchStateMachine)context;
    }

    public void Exit()
    {
    }
}