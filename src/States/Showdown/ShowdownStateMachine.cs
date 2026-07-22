using System;
using System.Collections.Generic;
using Showdown4.Commands;
using Showdown4.Entities;
using Showdown4.Managers;
using UnityEngine;
using ZeepkistClient;
using ZeepSDK.Level;
using ZeepSDK.Multiplayer;
using ZeepSDK.Racing;

namespace Showdown4.States.Showdown;

public class ShowdownStateMachine : MonoBehaviour, IStateMachine
{
	public Match Match { get; set; }

	// Always return new instances for InitialState and FinalState
	public IState InitialState => new StateShowdownStarting(this);
	public IState FinalState => new StateMatchEnd(this);
	public IState CurrentState { get; set; }
	public List<ITransition> Transitions { get; set; } = new();

	public event Action StateMachineFinished;

	public void InitTransitions()
	{
		IStateMachine stateMachine = this;
		Transitions = new List<ITransition>();
		// Transitions use new instances directly
		stateMachine
			.AddTransition(new StateShowdownStarting(this), new StateWaitingForHoF(this),
				() => !LevelApi.CurrentLevel.UID.Equals(
					PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)[0]
						.UID))
			.AddTransition(new StateShowdownStarting(this), new StateSetupMatch(this),
				() => LevelApi.CurrentLevel.UID.Equals(
					PlaylistManager.GetLocalLevelsByPlaylistName(MyConfig.IntermissionLevelPlaylistNameConfig.Value)[0]
						.UID))
			.AddTransition(new StateWaitingForHoF(this), new StateSetupMatch(this))
			.AddTransition(new StateSetupMatch(this), new StateLinkRacers(this))
			.AddTransition(new StateLinkRacers(this), new StateSelectInitiative(this))
			.AddTransition(new StateSelectInitiative(this), new StatePreDraft(this))
			.AddTransition(new StatePreDraft(this), new StateDrafting(this))
			.AddTransition(new StateDrafting(this), new StateDraftIncomplete(this),
				() => Match.CurrentDraft.PickedLevels.Count == 0)
			.AddTransition(new StateDrafting(this), new StateDraftCompleted(this))
			.AddTransition(new StateDraftIncomplete(this), new StateDraftCompleted(this))
			.AddTransition(new StateDraftCompleted(this), new StateReadyCheck(this))
			.AddTransition(new StateReadyCheck(this), new StatePreRacing(this))
			.AddTransition(new StatePreRacing(this), new StateRacing(this))
			.AddTransition(new StateRacing(this), new StatePostRacing(this))
			.AddTransition(new StatePostRacing(this), new StateRacing(this),
				() => Match.RoundCounter() < 2)
			.AddTransition(new StatePostRacing(this), new StatePreDraft(this),
				() => Match.RoundCounter() >= 2 && Match.TeamA.Wins < 2 && Match.TeamB.Wins < 2)
			.AddTransition(new StatePostRacing(this), new StateMatchEnd(this),
				() => Match.TeamA.Wins >= 2 || Match.TeamB.Wins >= 2)
			.AddTransition(new StateMatchEnd(this), new StateWaitingForHoF(this));
	}

	public void InvokeFinish()
	{
		StateMachineFinished?.Invoke();
	}

	public void SubscribeToEvents()
	{
		RacingApi.RoundStarted += HandleRoundStarted;
		RacingApi.RoundEnded += HandleRoundEnded;
		RacingApi.LevelLoaded += HandleLevelLoaded;
		ZeepkistNetwork.PlayerResultsChanged += HandlePlayerResultsChanged;
		MultiplayerApi.PlayerJoined += HandlePlayerJoined;

		CommandBan.CommandInvoked += HandleBan;
		CommandPick.CommandInvoked += HandlePick;
		CommandLinkRacer.CommandInvoked += HandleLinkRacer;
		CommandUnLinkRacer.CommandInvoked += HandleUnlinkRacer;
		CommandPass.CommandInvoked += HandlePass;
		CommandReady.CommandInvoked += HandleReady;
	}

	public void UnsubscribeFromEvents()
	{
		RacingApi.RoundStarted -= HandleRoundStarted;
		RacingApi.RoundEnded -= HandleRoundEnded;
		RacingApi.LevelLoaded -= HandleLevelLoaded;
		ZeepkistNetwork.PlayerResultsChanged -= HandlePlayerResultsChanged;
		MultiplayerApi.PlayerJoined -= HandlePlayerJoined;

		CommandBan.CommandInvoked -= HandleBan;
		CommandPick.CommandInvoked -= HandlePick;
		CommandLinkRacer.CommandInvoked -= HandleLinkRacer;
		CommandUnLinkRacer.CommandInvoked -= HandleUnlinkRacer;
		CommandPass.CommandInvoked -= HandlePass;
		CommandReady.CommandInvoked -= HandleReady;
	}

	private void HandleRoundStarted()
	{
		CurrentState?.OnRoundStarted();
	}

	private void HandleRoundEnded()
	{
		CurrentState?.OnRoundEnded();
	}

	private void HandleLevelLoaded()
	{
		CurrentState?.OnLevelLoaded();
	}

	private void HandlePlayerResultsChanged(ZeepkistNetworkPlayer player)
	{
		CurrentState?.OnPlayerResultsChanged(player);
	}

	private void HandlePlayerJoined(ZeepkistNetworkPlayer player)
	{
		CurrentState?.OnPlayerJoined(player);
	}

	private void HandleBan(ulong steamId, string levelIndex)
	{
		CurrentState?.OnBan(steamId, levelIndex);
	}

	private void HandlePick(ulong steamId, string levelIndex)
	{
		CurrentState?.OnPick(steamId, levelIndex);
	}

	private void HandleLinkRacer(ulong steamId)
	{
		CurrentState?.OnLinkRacer(steamId);
	}

	private void HandleUnlinkRacer(ulong steamId)
	{
		CurrentState?.OnUnlinkRacer(steamId);
	}

	private void HandlePass(ulong steamId)
	{
		CurrentState?.OnPass(steamId);
	}

	private void HandleReady(ulong steamId, string arguments)
	{
		CurrentState?.OnReady(steamId, arguments);
	}
}