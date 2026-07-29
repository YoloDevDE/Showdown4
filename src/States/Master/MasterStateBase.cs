namespace Showdown4.States.Master;

/// <summary>
///     Base class for every state that runs inside the <see cref="MasterStateMachine" />.
///     All the shared plumbing lives in <see cref="StateBase" />; the master states only add the
///     nested <see cref="StateBase.SubStateMachine" /> (the showdown machine) on top of it.
/// </summary>
public abstract class MasterStateBase(IStateMachine stateMachine) : StateBase(stateMachine);