namespace Showdown4.Statemachine;

public class StateSettingTeam : IState
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