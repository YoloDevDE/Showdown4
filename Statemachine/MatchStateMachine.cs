using Showdown4.Commands;
using Showdown4.Tmp;
using ZeepSDK.Messaging;

namespace Showdown4.Statemachine;

public class MatchStateMachine : BaseStatemachine

{
    public Match CurrentMatch;
    public bool isRunning;

    public MatchStateMachine()
    {
        CommandStartMatch.CommandInvoked += OnStartMatch;
        CommandStopMatch.CommandInvoked += OnStopMatch;
    }

    public override IState State { get; set; }


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
            StartStatMachine(this, new State_SetTeams());
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