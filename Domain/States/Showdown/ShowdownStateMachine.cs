using System;
using System.Collections.Generic;
using Showdown4.Tmp;

namespace Showdown4.Domain.States.Showdown;

public class ShowdownStateMachine : IStateMachine
{
    public ShowdownStateMachine()
    {
        InitialState = new State_Showdown_Starting(this);
        FinalState = new State_Match_End(this);
        Timer = new Timer();
        Transitions = [];
    }

    public Match Match { get; set; }

    public Timer Timer { get; }

    public IState CurrentState { get; set; }
    public IState InitialState { get; }
    public IState FinalState { get; }
    public List<ITransition> Transitions { get; }
    public event Action StateMachineFinished;

    public void InitTransitions()
    {
        IStateMachine stateMachine = this;
        IState stateShowdownStarting = new State_Showdown_Starting(this);
        IState stateSetupMatch = new State_SetupMatch(this);
        IState stateLinkRacers = new State_LinkRacersToTeams(this);
        IState statePreRacing = new State_PreRacing(this);
        IState stateRacing = new State_Racing(this);
        IState statePostRacing = new State_PostRacing(this);
        IState stateRaceEvaluation = new State_RaceEvaluation(this);
        IState stateMatchEnd = new State_Match_End(this);
        IState stateSelectInitiative = new State_SelectInitiative(this);
        IState stateDrafting = new State_Drafting(this);
        IState stateWaitingForHoF = new State_WaitingForHoF(this);
        stateMachine
            .AddTransition(Transition.CreateInstance(InitialState, stateWaitingForHoF))
            .AddTransition(Transition.CreateInstance(stateWaitingForHoF, stateSetupMatch))
            .AddTransition(Transition.CreateInstance(stateSetupMatch, stateLinkRacers))
            .AddTransition(Transition.CreateInstance(stateLinkRacers, stateDrafting))
            ;
    }

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}