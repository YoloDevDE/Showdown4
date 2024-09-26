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
        Transitions = new List<ITransition>();
    }

    public Match Match { get; set; }

    // Always return new instances for InitialState and FinalState
    public IState InitialState => new State_Showdown_Starting(this);
    public IState FinalState => new State_Match_End(this);
    public IState CurrentState { get; set; }
    public List<ITransition> Transitions { get; }

    public event Action StateMachineFinished;

    public void InitTransitions()
    {
        IStateMachine stateMachine = this;

        // Transitions use new instances directly
        stateMachine
            .AddTransition(new State_Showdown_Starting(this), new State_WaitingForHoF(this),
                () => !LevelApi.CurrentLevel.UID.Equals(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
            .AddTransition(new State_Showdown_Starting(this), new State_SetupMatch(this),
                () => LevelApi.CurrentLevel.UID.Equals(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
            .AddTransition(new State_WaitingForHoF(this), new State_SetupMatch(this))
            .AddTransition(new State_SetupMatch(this), new State_LinkRacers(this))
            .AddTransition(new State_LinkRacers(this), new State_SelectInitiative(this))
            .AddTransition(new State_SelectInitiative(this), new State_Drafting(this))
            .AddTransition(new State_Drafting(this), new State_PostDrafting(this))
            .AddTransition(new State_PostDrafting(this), new State_PreRacing(this))
            .AddTransition(new State_PreRacing(this), new State_Racing(this))
            .AddTransition(new State_Racing(this), new State_PostRacing(this))
            .AddTransition(new State_PostRacing(this), new State_Racing(this),
                () => Match.RoundCounter() < 2)
            .AddTransition(new State_PostRacing(this), new State_Drafting(this),
                () => Match.RoundCounter() >= 2 && Match.TeamA.Wins < 2 && Match.TeamB.Wins < 2)
            .AddTransition(new State_PostRacing(this), new State_Match_End(this),
                () => Match.TeamA.Wins >= 2 || Match.TeamB.Wins >= 2)
            .AddTransition(new State_Match_End(this), new State_Showdown_Starting(this));
    }

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}