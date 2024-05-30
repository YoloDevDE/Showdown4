using Showdown4.Service;

namespace Showdown4.Statemachine;

public class InitializeMatch : BaseStatemachine
{
    public Match CurrentMatch { get; set; }
    public override IState State { get; set; }

    public override void StopStateMachine()
    {
        State?.Exit();
    }
}