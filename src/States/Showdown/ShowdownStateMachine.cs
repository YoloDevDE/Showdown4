using System;
using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Managers;
using ZeepSDK.Level;

namespace Showdown4.States.Showdown;

public class ShowdownStateMachine : IStateMachine
{
    public ShowdownStateMachine()
    {
        InitialState = new State_Showdown_Starting(this);
        FinalState = new State_Match_End(this);
        Transitions = [];
    }

    public Match Match { get; set; }


    public IState CurrentState { get; set; }
    public IState InitialState { get; }
    public IState FinalState { get; }
    public List<ITransition> Transitions { get; }
    public event Action StateMachineFinished;

    public void InitTransitions()
    {
        IStateMachine stateMachine = this;
        IState stateShowdownStarting = InitialState;
        IState stateSetupMatch = new State_SetupMatch(this);
        IState stateLinkRacers = new State_LinkRacers(this);
        IState statePreRacing = new State_PreRacing(this);
        IState stateRacing = new State_Racing(this);
        IState statePostRacing = new State_PostRacing(this);
        IState stateRaceEvaluation = new State_RaceEvaluation(this);
        IState stateMatchEnd = new State_Match_End(this);
        IState stateSelectInitiative = new State_SelectInitiative(this);
        IState stateDrafting = new State_Drafting(this);
        IState stateWaitingForHoF = new State_WaitingForHoF(this);
        IState statePostDrafting = new State_PostDrafting(this);
        stateMachine
            .AddTransition(Transition.CreateInstance(stateShowdownStarting, stateWaitingForHoF, () => !LevelApi.CurrentLevel.UID.Equals(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
            )
            .AddTransition(Transition.CreateInstance(stateShowdownStarting, stateSetupMatch, () => LevelApi.CurrentLevel.UID.Equals(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID)))
            .AddTransition(Transition.CreateInstance(stateWaitingForHoF, stateSetupMatch))
            .AddTransition(Transition.CreateInstance(stateSetupMatch, stateLinkRacers))
            .AddTransition(Transition.CreateInstance(stateLinkRacers, stateDrafting))
            .AddTransition(Transition.CreateInstance(stateDrafting, statePostDrafting))
            .AddTransition(Transition.CreateInstance(statePostDrafting, statePreRacing))
            .AddTransition(Transition.CreateInstance(statePreRacing, stateRacing))
            .AddTransition(Transition.CreateInstance(stateRacing, statePostRacing))
            .AddTransition(Transition.CreateInstance(statePostRacing, stateRacing, () => Match.RoundCounter() < 2))
            .AddTransition(Transition.CreateInstance(statePostRacing, stateDrafting, () => Match.RoundCounter() >= 2 && Match.TeamA.Wins < 2 && Match.TeamB.Wins < 2))
            .AddTransition(Transition.CreateInstance(statePostRacing, stateMatchEnd, () => Match.TeamA.Wins >= 2 || Match.TeamB.Wins >= 2))
            .AddTransition(Transition.CreateInstance(stateMatchEnd, stateShowdownStarting))
            ;
    }

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}