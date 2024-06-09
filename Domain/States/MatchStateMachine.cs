using Showdown4.Tmp;
using ZeepSDK.Messaging;

namespace Showdown4.Domain.States;

public class MatchStateMachine : BaseStateMachine<IState>
{
    public Match CurrentMatch;
    public bool isRunning;


    public IState State { get; set; }


    private void OnStopMatch()
    {
        if (isRunning)
        {
            MessengerApi.Log("Showdown Match Stopped");
            StopStateMachine();
        }
        else
        {
            MessengerApi.Log("Showdown Match is not Running");
        }
    }

    private void OnStartMatch()
    {
        if (!isRunning)
        {
            MessengerApi.Log("Showdown Match Started");
            isRunning = !isRunning;
        }
        else
        {
            MessengerApi.Log("Showdown Match already Started");
        }
    }


    public override void StopStateMachine()
    {
        MessengerApi.Log("Showdown Match Stopped");
        State?.Exit();
        isRunning = !isRunning;
    }
}