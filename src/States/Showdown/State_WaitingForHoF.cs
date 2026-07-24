using Showdown4.Managers;

namespace Showdown4.States.Showdown;

public class StateWaitingForHoF(IStateMachine stateMachine) : ShowdownStateBase(stateMachine)
{
	public override void Enter()
	{
		if (PlaylistManager.IsOnIntermissionLevel())
		{
			OnRoundStarted();
		}
	}

	public override void Exit()
	{
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