using System;
using System.Collections.Generic;
using Showdown4.Entities;
using Showdown4.Managers;
using UnityEngine;
using ZeepSDK.Level;

namespace Showdown4.States.Showdown;

public class ShowdownStateMachine : MonoBehaviour, IStateMachine
{
    public ShowdownStateMachine()
    {
        Transitions = new List<ITransition>();
    }

    public Match Match { get; set; }

    public IState InitialState => new State_ShowdownStarted(this);
    public IState FinalState => new State_MatchOver(this);
    public IState CurrentState { get; set; }
    public List<ITransition> Transitions { get; set; }

    public event Action StateMachineFinished;

    public void InitTransitions()
    {
        IStateMachine stateMachine = this;
        Transitions = new List<ITransition>();

        stateMachine
            .AddTransition(new State_ShowdownStarted(this), new State_WaitingForIntermission(this),
                () => !LevelApi.CurrentLevel.UID.Equals(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
            .AddTransition(new State_ShowdownStarted(this), new State_SetupMatch(this),
                () => LevelApi.CurrentLevel.UID.Equals(PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
            .AddTransition(new State_WaitingForIntermission(this), new State_SetupMatch(this))
            .AddTransition(new State_SetupMatch(this), new State_LinkRacers(this))
            .AddTransition(new State_LinkRacers(this), new State_SelectInitiative(this))
            .AddTransition(new State_SelectInitiative(this), new State_PreDraftCountdown(this))
            .AddTransition(new State_PreDraftCountdown(this), new State_Drafting(this))
            .AddTransition(new State_Drafting(this), new State_DraftComplete(this),
                () => Match.CurrentDraft.IsDraftComplete())
            .AddTransition(new State_Drafting(this), new State_DraftIncomplete(this),
                () => !Match.CurrentDraft.IsDraftComplete())
            .AddTransition(new State_DraftComplete(this), new State_PreRoundReadyCheck(this))
            .AddTransition(new State_DraftIncomplete(this), new State_RandomMapSelection(this))
            .AddTransition(new State_RandomMapSelection(this), new State_PreRoundReadyCheck(this))
            .AddTransition(new State_PreRoundReadyCheck(this), new State_PreRoundCountdown(this))
            .AddTransition(new State_PreRoundCountdown(this), new State_RoundStarted(this))
            .AddTransition(new State_RoundStarted(this), new State_RoundEnded(this))
            .AddTransition(new State_RoundEnded(this), new State_RoundStarted(this),
                () => Match.RoundCounter() < 2)
            .AddTransition(new State_RoundEnded(this), new State_Drafting(this),
                () => Match.RoundCounter() >= 2 && Match.TeamA.Wins < 2 && Match.TeamB.Wins < 2)
            .AddTransition(new State_RoundEnded(this), new State_MatchOver(this),
                () => Match.TeamA.Wins >= 2 || Match.TeamB.Wins >= 2)
            .AddTransition(new State_MatchOver(this), new State_WaitingForIntermission(this));
    }

    public void InvokeFinish()
    {
        StateMachineFinished?.Invoke();
    }
}