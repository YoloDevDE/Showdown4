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

	// Always return new instances for InitialState and FinalState
	public IState InitialState => new StateShowdownStarting(this);
	public IState FinalState => new StateMatchEnd(this);
	public IState CurrentState { get; set; }
	public List<ITransition> Transitions { get; set; }

	public event Action StateMachineFinished;

	public void InitTransitions()
	{
		IStateMachine stateMachine = this;
		Transitions = new List<ITransition>();
		// Transitions use new instances directly
		stateMachine
			.AddTransition(new StateShowdownStarting(this), new StateWaitingForHoF(this),
				() => !LevelApi.CurrentLevel.UID.Equals(
					PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
			.AddTransition(new StateShowdownStarting(this), new StateSetupMatch(this),
				() => LevelApi.CurrentLevel.UID.Equals(
					PlaylistManager.GetLocalLevelsByPlaylistName(Plugin.IntermissionLevelPlaylistName.Value)[0].UID))
			.AddTransition(new StateWaitingForHoF(this), new StateSetupMatch(this))
			.AddTransition(new StateSetupMatch(this), new StateLinkRacers(this))
			.AddTransition(new StateLinkRacers(this), new StateSelectInitiative(this))
			.AddTransition(new StateSelectInitiative(this), new StateDrafting(this))
			.AddTransition(new StateDrafting(this), new StateReadyCheck(this))
			.AddTransition(new StateReadyCheck(this), new StatePreRacing(this))
			.AddTransition(new StatePreRacing(this), new StateRacing(this))
			.AddTransition(new StateRacing(this), new StatePostRacing(this))
			.AddTransition(new StatePostRacing(this), new StateRacing(this),
				() => Match.RoundCounter() < 2)
			.AddTransition(new StatePostRacing(this), new StateDrafting(this),
				() => Match.RoundCounter() >= 2 && Match.TeamA.Wins < 2 && Match.TeamB.Wins < 2)
			.AddTransition(new StatePostRacing(this), new StateMatchEnd(this),
				() => Match.TeamA.Wins >= 2 || Match.TeamB.Wins >= 2)
			.AddTransition(new StateMatchEnd(this), new StateWaitingForHoF(this));
	}

	public void InvokeFinish()
	{
		StateMachineFinished?.Invoke();
	}
}