using Showdown4.Config;
using Showdown4.Utils;

namespace Showdown4.States.Master;

public class StateMasterOff(IStateMachine stateMachine) : MasterStateBase(stateMachine)
{
	public override IState GetNextState()
	{
		return new StateMasterOn(StateMachine);
	}

	public override void OnShowdownStop()
	{
		ToastMessenger.LogWarning("already stopped");
	}

	public override void OnShowdownStart()
	{
		InvokeFinish();

		ToastMessenger.LogSuccess($"Season {MyConfig.SeasonNumberConfig.Value} started");
	}
}