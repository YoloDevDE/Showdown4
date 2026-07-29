using Showdown4.Managers;

namespace Showdown4.States.Showdown;

/// <summary>
///     Waits until the lobby is back on the Hall of Fame (intermission) level. If it already is,
///     this state finishes immediately.
/// </summary>
public class StateWaitingForHoF(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		if (PlaylistManager.IsOnIntermissionLevel())
		{
			InvokeFinish();
		}
	}

	public override void OnRoundStarted()
	{
		InvokeFinish();
	}

	public override IState GetNextState()
	{
		return new StateSetupMatch(StateMachine);
	}
}